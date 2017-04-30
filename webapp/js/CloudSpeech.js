var cloud = window.cloud || {};
cloud.Speech = function () {
    this.API_KEY = "AIzaSyBmkQCRCoqCxhkiioxspVc6iWXX0KC569w";
    this.SERVICE_URL = "https://speech.googleapis.com/v1beta1/speech:syncrecognize?key=" + this.API_KEY;
    this.MAX_RECORD_TIME = 3E4;
    this.TIMEOUT_AMOUNT = 6E4;
    this.RECORD_END_DELAY = 500;
    this.NO_CAPTCHA_SUBMITS_ALLOWED = 5;
    this.recordButton_ = document.getElementById("btnRecord");
    this.timerCaption_ = document.getElementById("speech_record_timer");
    this.statusContainer_ = document.getElementById("speech_record_status");
    this.isProcessing_ = this.isRecording_ = !1;
    this.totalSubmits_ = this.startTime_ = this.submitTimeout_ = 0;
    this.rec_ = this.audioContext_ = null
};

// cloud.Speech.prototype.init = function () {
//     this.supportsFileReader() ? this.recordButton_.addEventListener("click", function (a) {
//         a.preventDefault();
//         this.audioContext_ ? this.toggleRecord() : this.initRecorder()
//     }.bind(this)) : this.hideDemo()
// };

cloud.Speech.prototype.init = function() {
    $(cloud.speechDemo.recordButton_).on('mousedown touchstart', startRecording);

    function startRecording(){
        $(cloud.speechDemo.recordButton_).addClass('recording');

        $(window).mouseup(stopRecording);
        $(cloud.speechDemo.recordButton_).on('touchend', stopRecording);

        cloud.speechDemo.audioContext_ ? cloud.speechDemo.toggleRecord() : cloud.speechDemo.initRecorder();
    }

    function stopRecording(){
        $(cloud.speechDemo.recordButton_).removeClass('recording');

        cloud.speechDemo.toggleRecord();

        $(window).unbind('mouseup', stopRecording);
        $(cloud.speechDemo.recordButton_).unbind('touchend', stopRecording);

        addComment('Thinking about what you said...');
    }
};



cloud.Speech.prototype.initRecorder = function () {
    try {
        window.AudioContext = window.AudioContext || window.webkitAudioContext, navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia || navigator.msGetUserMedia, window.URL = window.URL || window.webkitURL, this.audioContext_ = new AudioContext
    } catch (a) {
        console.log(a);
    }
    navigator.getUserMedia({
        audio: !0
    }, function (a) {
        this.startUserMedia(a)
    }.bind(this), function () {
        this.hideDemo()
    }.bind(this))
};
cloud.Speech.prototype.hideDemo = function () {
    
};
cloud.Speech.prototype.processAudioRecording = function (a) {
    var b = 'en-AU',
        c = new FileReader;
    c.onload = function (a) {
        a = a.target.result;
        this.sendAudio(btoa(a), b, "LINEAR16", this.audioContext_.sampleRate)
    }.bind(this);
    c.readAsBinaryString(a)
};
cloud.Speech.prototype.enableForm = function () {
    this.recordButton_.removeAttribute("disabled")
};
cloud.Speech.prototype.sendAudio = function (a, b, c, e) {
    a = JSON.stringify({
        config: {
            encoding: c,
            sampleRate: e,
            languageCode: b,
            maxAlternatives: 1
        },
        audio: {
            content: a
        }
    });
    var d = new XMLHttpRequest;
    d.onload = function (a) {
        200 <= d.status && 400 > d.status ? (a = JSON.parse(d.responseText), this.showResults(a)) : this.handleError(a)
    }.bind(this);
    d.onerror = this.handleError;
    d.open("POST", this.SERVICE_URL, !0);
    d.send(a);
    this.requestTimedOut = !1;
    this.submitTimeout_ = setTimeout(function () {
        this.showError();
        this.requestTimedOut = !0
    }.bind(this),
        this.TIMEOUT_AMOUNT)
};
cloud.Speech.prototype.handleError = function (a) {
    this.requestTimedOut || (clearTimeout(this.submitTimeout_), this.enableForm(), this.isProcessing_ = !1, console.log(a))
};
cloud.Speech.prototype.showError = function () {
    clearTimeout(this.submitTimeout_);
    this.enableForm();
    this.isProcessing_ = !1;
    this.setButtonState("");
};
cloud.Speech.prototype.showResults = function (a) {
    this.requestTimedOut || (clearTimeout(this.submitTimeout_), this.enableForm(), this.isProcessing_ = !1,
    //console.log(JSON.stringify(a, null, 2)),
    a.results ? (a = a.results.map(function (a) {
        return a.alternatives[0].transcript
    }),
    $('.question-input').val(a.join(""))) : $('.question-input').val('')),
    $('.question-input').focus();
    this.setButtonState("");

    console.log('speech: ', a);
    submitQuestion();
};
cloud.Speech.prototype.toggleRecord = function () {
    this.rec_ ? this.isRecording_ ? this.stopRecording() : this.isProcessing_ || this.startRecording() : this.hideDemo()
};
cloud.Speech.prototype.startRecording = function () {
    var a, b = 0,
        c = "00",
        e = " / 00:" + this.MAX_RECORD_TIME.toString().slice(0, -3);
    
    this.setButtonState("recording");
    this.timerCaption_.textContent = "00:00" + e;
    this.startTime_ = Date.now();
    this.rec_.clear();
    this.rec_.record();
    this.isRecording_ = !0;
    a = setInterval(function () {
        this.isRecording_ ? (b = Date.now() - this.startTime_, b >= this.MAX_RECORD_TIME ? (this.stopRecording(), clearInterval(a)) :
            1E3 <= b && (c = 1E4 > b ? "0" + b.toString().slice(0, -3) : b.toString().slice(0, -3), this.timerCaption_.textContent = "00:" + c + e)) : clearInterval(a)
    }.bind(this), 250)
};
cloud.Speech.prototype.stopRecording = function () {
    this.isProcessing_ = !0;
    this.isRecording_ = !1;
    this.setButtonState("processing");
    setTimeout(function () {
        this.rec_.stop();
        this.captchaSuccess()
    }.bind(this), this.RECORD_END_DELAY)
};
cloud.Speech.prototype.setButtonState = function (a) {
    switch (a) {
        case "processing":
            this.statusContainer_.textContent = "processing";
            this.timerCaption_.style.display = "none";
            this.statusContainer_.style.display = "block";
            break;
        case "recording":
            this.statusContainer_.textContent = "recording";
            this.timerCaption_.style.display = "block";
            this.statusContainer_.style.display = "block";
            break;
        default:
            this.statusContainer_.textContent = "",
            this.statusContainer_.style.display = "none",
            this.timerCaption_.style.display = "none"
    }
};
cloud.Speech.prototype.captchaSuccess = function () {
    this.rec_.exportWAV(function (a) {
        this.processAudioRecording(a)
    }.bind(this))
};
cloud.Speech.prototype.needsCaptcha = function () {
    return false;
};
cloud.Speech.prototype.startUserMedia = function (a) {
    a = this.audioContext_.createMediaStreamSource(a);
    this.rec_ = new window.Recorder(a, {
        numChannels: 1,
        workerPath: "js/recorderWorker-bundle.js"
    });
    this.toggleRecord()
};
cloud.Speech.prototype.supportsFileReader = function () {
    return "FileReader" in window
};
window.globalCaptchaSuccess = function () {
    cloud.speechDemo.captchaSuccess()
};
cloud.speechDemo = new cloud.Speech;
cloud.speechDemo.init();