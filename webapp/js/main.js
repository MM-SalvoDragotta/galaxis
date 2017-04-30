


function guid() {
	function s4() {
		return Math.floor((1 + Math.random()) * 0x10000)
			.toString(16)
			.substring(1);
	}
	return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
		s4() + '-' + s4() + s4() + s4();
}



var totalScrollHeight = 0;
var scrollTime = 500;
var userId = guid();


// Bind event handler for most recent input field
$('.main-form').on('keyup', '.form-entry.current-entry input', function(event) {
	// Ignore anything but enter key
	if(event.keyCode != 13) {
		return;
	}

	submitQuestion();
});


function submitQuestion() {
	// Disable the input field
	$('.main-form .form-entry.current-entry input').prop('readonly', 'readonly');

	var question = $('.main-form .form-entry.current-entry input').val();

	var postData = {
		"SessionId" : userId,
		"Query" : question
	}

	addComment('Thinking about your question...');

	$.post('https://nasaai.azurewebsites.net/api/message', postData, function(response) {
		console.log('response: ', response);
		//addAnswer(decodeURIComponent(response.Response));

		var answer;
		try {
			answer = decodeURIComponent(response.Response);
		}
		catch(exception) {
			answer = response.Response;
		}

		addAnswer(answer);

	});
}


var bunchOfAnswers = [
	'Lorem Ipsum is simply dummy text of the printing and typesetting industry.',
	'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.',
	'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.'
];

function getRandomAnswer() {
	var index = Math.floor((Math.random() * bunchOfAnswers.length));
	return bunchOfAnswers[index];
}

function addAnswer(answerText) {

	$('.main-form .form-entry').each(function() {
		if($(this).hasClass('fade1')) {
			$(this).removeClass('fade1').addClass('fade2');
		}
		else {
			$(this).addClass('fade1');
		}
	});

	$('.main-form .form-entry.current-entry input').removeClass('question-input');

	var template = $('#answer-template').html();
	var answerHtml = Mustache.render(template, { 'answerText': answerText });
	$('.main-form').append(answerHtml);

	scrollNewAnswer();

	// Fade in new entry
	setTimeout(function() {
		$('.main-form .form-entry').removeClass('current-entry');
		$('.main-form .form-entry:last-child').removeClass('new-entry').addClass('current-entry');
		$('.main-form .form-entry.current-entry input').focus();

		var htmlStripped = $('<p>' + answerText + '</p>').text();

		ttsService.ConvertToAudio(htmlStripped);
	}, scrollTime);
	
}

function addComment(commentText) {
	$('.main-form .form-entry').each(function() {
		if($(this).hasClass('fade1')) {
			$(this).removeClass('fade1').addClass('fade2');
		}
		else {
			$(this).addClass('fade1');
		}
	});

	var template = $('#comment-template').html();
	var commentHtml = Mustache.render(template, { 'commentText': commentText });
	$('.main-form').append(commentHtml);

	scrollNewAnswer();
}


function scrollNewAnswer() {
	var height = $('.main-form .form-entry:last-child').outerHeight();
	totalScrollHeight += height;
	$('.main-form').css('transform', 'translate(0px, -' + totalScrollHeight + 'px)');
}

function initialise() {
	var openingQuestions = [
		"Hi there. My name is <strong>GALAXIS</strong>. <br> I'm your virtual space buddy. Think of me like Steven Hawking pocket edition for space. <br> <br> You can ask me questions about Mars, Earth, Climate Change... plus I can do some space math calculation.",
		"Hey! I'm <strong>GALAXIS</strong>, nice to meet you :) <br> I'm like a virtual Rick from Rick and Morty... but a sober one. <br> <br> I know all about Earth, Mars and I can do some space math.",
		"Hi there. I'm <strong>GALAXIS</strong>! <br> I'm your guide to understanding our universe. I can be your space buddy and together we can learn about the planets in our solar system, what space travel is like and why the earth is such a special planet.",
		"Hi there. I'm <strong>GALAXIS</strong>! <br> If you've ever imagined what space travel is like, thought about living on Mars or just simply curious about our solar system. We can learn about it together and discover why our Earth is so special.",
		"Hey there, my name is <strong>GALAXIS</strong> and I'm your space buddy. <br> You can ask me anything about the universe and our solar system. We can even talk travelling to other planets!"
	];
	var index = Math.floor((Math.random() * openingQuestions.length));
	//index = 4;
	addAnswer(openingQuestions[index]);
}
initialise();



var wordCloud = [
	{ text: 'Hi there!', weight: 1 },
	{ text: 'Lorem ipsum', weight: 1 },
	{ text: 'Something else', weight: 1 },
	{ text: 'Do the thing!', weight: 1 },
	{ text: 'Abra cadabra', weight: 1 },
];

$('#tag-cloud').jQCloud(wordCloud);
