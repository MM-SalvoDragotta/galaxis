var ttsService = (function (ttsService, $) {

    ttsService = {

        ConvertToAudio: function(text) {
            $.ajax({
                type: "POST",
                url: AccessTokenUri,
                headers: {
                    'Ocp-Apim-Subscription-Key': apiKey
                },
                success: function (data) {
                    accessToken = data;
                    //console.log(accessToken);
                    if (accessToken != '') {
                        sendAudioRequest(text);
                    }
                },
            });
        }
    }



    var apiKey = "4a47c14e828f40b8801c67212766e0f5";
    var post_data = "";
    var accessToken = "";

    var AccessTokenUri = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";

    var audioURL = "https://speech.platform.bing.com/synthesize";

    var context = new AudioContext();
    var speechBuffer = null;
    var source = null;

    function sendAudioRequest(text) {
        sendString = "<speak version='1.0' xml:lang='en-US'><voice xml:lang='en-GB' xml:gender='Female' name='Microsoft Server Speech Text to Speech Voice (en-GB, Susan, Apollo)'>" + text + "</voice></speak>";
        
        if (source != null)
        {
            source.stop();
        }

        var xhttp = new XMLHttpRequest();

        xhttp.onreadystatechange = function () {
            if (xhttp.readyState == 4 && xhttp.status == 200) {
                context.decodeAudioData(xhttp.response, function (buffer) {
                    speechBuffer = buffer;
                    playAudio(speechBuffer);
                });

            }
        };

        xhttp.open("POST", audioURL, true);
        xhttp.setRequestHeader("Content-type", 'application/ssml+xml');
        xhttp.setRequestHeader("Authorization", 'Bearer ' + accessToken);
        xhttp.setRequestHeader("X-Microsoft-OutputFormat", 'riff-16khz-16bit-mono-pcm');
        xhttp.setRequestHeader("X-Search-AppId", '07D3234E49CE426DAA29772419F436CA');
        xhttp.setRequestHeader("X-Search-ClientID", '1ECFAE91408841A480F00935DC390960');
        xhttp.responseType = 'arraybuffer'

        xhttp.send(sendString);
    }

    function playAudio() {
        if (context == null) {
            context = new AudioContext();
        }

        source = context.createBufferSource();
        source.buffer = speechBuffer;
        source.connect(context.destination);
        source.playbackRate.value = 1.1;
        source.start(0);
    }
    
    return ttsService;

})(ttsService || {}, jQuery);