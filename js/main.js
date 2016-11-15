$(document).ready(function () {
    loadWrongQuestions();
    $('#challenge').hide();
    loadLocalFile();
    $.getJSON('dir.json?v=3', function (data) {
        var list = $('<div>').appendTo($("#files"));
        data.files.forEach(function (file) {
            list.append('<label>' + file.replace('.csv', '') + '</label>');
            $('<button type="button" class="btn">Exercise (in order)</button>').appendTo(list).on('click', function () {
                loadFile(file, $(this), false, false);
                $(this).addClass('loading');
            });

            $('<button type="button" class="btn">Exercise (shuffle)</button>').appendTo(list).on('click', function () {
                loadFile(file, $(this), true, false);
                $(this).addClass('loading');
            });

            $('<button type="button" class="btn">Vocabulary table</button>').appendTo(list).on('click', function () {
                loadFile(file, $(this), false, true);
                $(this).addClass('loading');
            });
            list.append('<br>');
        });

        $('#lang1-2').on('change', function () {
            flipDirection = !($(this).is(':checked'));
            loadQuestion();
        });

        $('#lang2-1').on('change', function () {
            flipDirection = ($(this).is(':checked'));
            loadQuestion();
        });

        $('#submit').on('click', submit);

        $('#clear-wrong').on('click', function () {
            if (!confirm("Are you sure you want to clear your list of mistakes?"))
                return;
            clearWrongQuestions();
        });

        $('#practise-mistakes').on('click', function() {
            $('#wrongquestions').hide('slow');
            $('#show-mistakes').prop('checked', false);
            practiceMistakes();
        });
        $('#skip').on('click', nextQuestion);

        $('#show-mistakes').change(function() {
            if($(this).is(":checked")) {
                $('#wrongquestions').show('slow');
            }
            else {
                $('#wrongquestions').hide('slow');
            }
        });

        $("#answer").keyup(function(event){
            if(event.keyCode == 13){
                $("#submit").click();
            }
        });
    });
});

var topic = "";
var lang1 = [];
var lang2 = [];
var questionNum = 0;
var question = "";
var tries = 0;
var rightQuestions = 0;
var flipDirection = false;
var wrongQuestions = [];

function loadFile(file, button, doShuffle, showTable) {
    $('#challenge, #voctable').slideUp('fast', function () {
        lang1 = [];
        lang2 = [];
        $.get('csv/' + file, function (data) {
            var lines = data.split('\n');
            topic = lines[0];
            $('.topic').text(topic);
            lines.splice(0, 1);
            if (doShuffle) {
                shuffle(lines);
            }
            loadData(lines, showTable);
            saveFileLocal({topic: topic, lines: lines});
            saveState();
            button.removeClass('loading');
        });
    });
}

function loadData(lines, showTable) {
    for(var i = 0; i < lines.length; i++) {
        var line = lines[i];
        var split = line.split(';');
        if (split.length < 2)
            continue;
        lang1[i] = split[0];
        lang2[i] = split[1];
    }

    questionNum = 0;
    rightQuestions = 0;
    if (showTable) {
        loadVocTable();
        $('#voctable').slideDown('fast');
    }
    else {
        loadQuestion();
        $('#challenge').slideDown('fast');
    }
    $("#lang-direction").children().prop('disabled', false);
}

function loadLocalFile() {
    $('#challenge, #voctable').slideUp('fast', function () {
        var item = localStorage.getItem('lastfile');
        if (item === null)
            return;

        var parsed = JSON.parse(item);
        topic = parsed.topic;
        $('.topic').text(topic);
        loadData(parsed.lines, false);
        loadState();
    });
}

function loadQuestion() {
    var lengthPercent = questionNum / lang1.length * 100;
    $('#progress-bar').css('width', lengthPercent + "%").text((questionNum) + " of " + lang1.length);
    tries = 0;
    $('#answer').removeClass('right').removeClass('wrong').prop('readonly', false).val('').focus();
    $('#submit').removeClass('moveon').text('Submit');
    $('.topic').text(topic);
    question = (flipDirection ? lang1 : lang2)[questionNum];
    $('#question').text(question);
    updateWrongQuestions();
}

function nextQuestion() {
    questionNum++;
    if (questionNum > lang1.length - 1) {
        showResults();
        clearState();
        removeLocalFile();
        $('#challenge').slideUp('fast');
    }
    else {
        loadQuestion();
    }
}

function showResults() {
    alert("Congrats! You're done!\n" +
        "Right: " + rightQuestions + " of " + questionNum + " (" + Math.round(rightQuestions/questionNum*100) + "%)");
}

function submit() {
    if ($(this).hasClass('moveon')) {
        nextQuestion();
        saveState();
        return;
    }
    var answerElem = $('#answer');
    var answer = answerElem.val().trim();//.toLowerCase();
    //if (answer === '')
    //    return;

    var rightAnswerTemplate = (flipDirection ? lang2 : lang1)[questionNum].trim();
    var templateNoBrackets = rightAnswerTemplate.replace(/ *\([^)]*\)*/g, "").trim();
    var rightAnswers = templateNoBrackets.split('/');
    var rightAnswersWithoutTo = templateNoBrackets.replace('to ', '').split('/');

    var maxTries = 1;
    if(rightAnswers.indexOf(answer) !== -1 || rightAnswersWithoutTo.indexOf(answer) !== -1
        || answer === templateNoBrackets || answer === templateNoBrackets.replace('to ', '')) {
        answerElem.addClass('right').removeClass('wrong');
        if (tries <= maxTries) {
            rightQuestions++;
        }
        showAnswer(rightAnswerTemplate);
    }
    else {
        tries++;
        answerElem.addClass('wrong').effect('shake');
        $(this).text('Try again!');

        if (tries > maxTries) {
            var wrongQuestion = {question: question, answer: rightAnswerTemplate};
            if (!wrongQuestions.find(function (question) {
                    return question.question === wrongQuestion.question && question.answer === wrongQuestion.answer;
                })) {
                wrongQuestions.push(wrongQuestion);
            }
            showAnswer(rightAnswerTemplate);
        }
    }
    saveState();
}

function showAnswer(answer) {
    $('#answer').val(answer).prop('readonly', true);
    $('#submit').addClass('moveon').text('Move on');
}

function practiceMistakes() {
    $('#challenge, #voctable').slideUp('fast', function () {
        topic = "Practice your mistakes";

        lang1 = [];
        lang2 = [];
        for (var i = 0; i < wrongQuestions.length; i++) {
            lang2[i] = wrongQuestions[i].question;
            lang1[i] = wrongQuestions[i].answer;
        }

        questionNum = 0;
        rightQuestions = 0;
        flipDirection = false;
        loadQuestion();
        $('#challenge').slideDown('fast');
        $("#lang1-2").prop('checked', true);
        $("#lang-direction").children().prop('disabled', true);
    });
}

function loadVocTable() {
    var content = '';
    lang1.forEach(function (question, index) {
        content += '<tr><td>' + question + '</td><td>' + lang2[index] + '</td>' + '</tr>';
    });
    $('#voctable-table').html(content);
}

function saveFileLocal(contents) {
    localStorage.setItem('lastfile', JSON.stringify(contents));
}

function removeLocalFile() {
    localStorage.removeItem('lastfile');
}

function loadState() {
    var state = localStorage.getItem('state');
    if (state === null)
        return;

    var parsed = JSON.parse(state);
    questionNum = parsed.questionNum;
    question = parsed.question;
    rightQuestions = parsed.rightQuestions;
    flipDirection = parsed.flipDirection;
    tries = parsed.tries;
    loadQuestion();
}

function saveState() {
    var state = {questionNum: questionNum, question: question, rightQuestions: rightQuestions, flipDirection: flipDirection, tries: tries};
    localStorage.setItem('state', JSON.stringify(state));
}

function clearState() {
    localStorage.removeItem('state');
}

function loadWrongQuestions() {
    var item = localStorage.getItem('wrongquestions');
    if (item === null)
        return;
    wrongQuestions = JSON.parse(item).data;
    showWrongQuestions()
}

function updateWrongQuestions() {
    localStorage.setItem('wrongquestions', JSON.stringify({data: wrongQuestions}));
    showWrongQuestions();
}

function clearWrongQuestions() {
    wrongQuestions = [];
    localStorage.removeItem('wrongquestions');
    showWrongQuestions();
}

function showWrongQuestions() {
    var content = '';
    wrongQuestions.forEach(function (question, index) {
        content += '<tr><td>' + question.question + '</td><td>' + question.answer + '</td>' +
            '<td><button class="btn" type="button" data-index="' + index + '" onclick="removeMistake(' + index + ')">Remove</button></td></tr>';
    });
    $('#wrongquestions').html(content);
}

function removeMistake(index) {
    wrongQuestions.splice(index, 1);
    updateWrongQuestions();
}

function shuffle(a) {
    var j, x, i;
    for (i = a.length; i; i--) {
        j = Math.floor(Math.random() * i);
        x = a[i - 1];
        a[i - 1] = a[j];
        a[j] = x;
    }
}