var grades = [
    { grade : 1, min: .90 },
    { grade : 2, min: .79 },
    { grade : 3, min: .68 },
    { grade : 4, min: .50 },
    { grade : 5, min: .0 }
];

$(document).ready(function () {
    loadWrongQuestions();
    $('#challenge, #voctable, #mistakes, #results').hide();
    $('#load-container').fadeOut('fast');
    loadLocalFile();
    $.getJSON('dir.json?v=' + getRandVersion(), function (data) {
        var list = $('<table class="table table-responsive" id="file-list">').appendTo($("#files"));
        data.files.forEach(function (file) {
            var row = $('<tr></tr>').appendTo(list);
            row.append('<td><label>' + file.replace('.csv', '').replace(/_/g, ' ') + '</label></td>');
            $('<td><button type="button" class="btn">Exercise (in order)</button></td>').appendTo(row).on('click', function () {
                loadFile(file, $(this), false, false);
            });

            $('<td><button type="button" class="btn">Exercise (shuffle)</button></td>').appendTo(row).on('click', function () {
                loadFile(file, $(this), true, false);
            });

            $('<td><button type="button" class="btn">Vocabulary table</button></td>').appendTo(row).on('click', function () {
                loadFile(file, $(this), false, true);
            });

            $('<td><button type="button" class="btn">View/Download</button></td>').appendTo(row).on('click', function () {
                window.location = 'csv/' + file;
            });
        });
        list.fadeIn('fast');

        $('#lang1-2').on('change', function () {
            flipDirection = !($(this).is(':checked'));
            loadQuestion();
        });

        $('#lang2-1').on('change', function () {
            flipDirection = ($(this).is(':checked'));
            loadQuestion();
        });
        $('#lang_random').on('change', function () {
            randomDirection = ($(this).is(':checked'));
            loadQuestion();
        });

        $('#submit').on('click', submit);

        $('#clear-wrong').on('click', function () {
            if (!confirm("Are you sure you want to clear your list of mistakes?"))
                return;
            clearWrongQuestions();
        });

        $('#practise-mistakes').on('click', function() {
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

        $('.back').on('click', function () {
            showMainMenu();
            clearState();
            removeLocalFile();
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
var randomDirection = false;
var wrongQuestions = [];

function loadFile(file, button, doShuffle, showTable) {
    $('#files').addClass('loading');
    lang1 = [];
    lang2 = [];
    $.get('csv/' + file + "?v=" + getRandVersion(), function (data) {
        var lines = data.split('\n');
        topic = lines[0];
        $('.topic').text(topic);
        lines.splice(0, 1);
        
        if(lines[lines.length-1] == "") {
        	lines.splice(-1,1);
        }
        
        if (doShuffle) {
            shuffle(lines);
        }
        loadData(lines, showTable);
        saveFileLocal({topic: topic, lines: lines});
        saveState();
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
        $('#voctable').show();
    }
    else {
        loadQuestion();
        $('#challenge, #mistakes').show();
    }
    $("#lang-direction").show();
    $('#files').hide();
}

function loadLocalFile() {
    var item = localStorage.getItem('lastfile');
    if (item === null)
        return;

    var parsed = JSON.parse(item);
    topic = parsed.topic;
    $('.topic').text(topic);
    loadData(parsed.lines, false);
    loadState();
}

function loadQuestion() {
    var lengthPercent = (questionNum+1) / lang1.length * 100;
    $('#progress-bar').css('width', lengthPercent + "%").text((questionNum+1) + " of " + lang1.length);
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
    }
    else {
        loadQuestion();
    }
}

function showResults() {
    $('#results').show();
    $('#challenge').hide();
    $('#topic-result').text(topic);
    $('#question-num').text(questionNum);
    var percentage = rightQuestions/questionNum;
    $('#right-percentage').text(Math.round(percentage*100));
    $('#right-num').text(rightQuestions);
    $('#wrong-num').text(questionNum - rightQuestions);
    var currentGrade = 5;
    for (var i = 0; i < grades.length; i++) {
        var grade = grades[0];
        if (percentage >= grade.min) {
            currentGrade = grade.grade;
            break;
        }
    }
    $('#grade').text(currentGrade);
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
        answerElem.addClass('wrong').effect('shake', function () {
            answerElem.focus();
        });
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
    if(randomDirection) {
    	flipDirection = Math.random() < 0.5 ? true : false;
    }
    saveState();
}

function showAnswer(answer) {
    $('#answer').val(answer).prop('readonly', true);
    $('#submit').addClass('moveon').text('Move on');
}

function practiceMistakes() {
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
    $("#lang1-2").prop('checked', true);
    $("#lang-direction").slideUp();
    $('#wrongquestions').hide('slow');
    $('#results').hide();
    $('#challenge').show();
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
            '<td><button class="btn mistake" type="button" data-index="' + index + '" onclick="removeMistakeAnimate($(this), ' + index + ')">Remove</button></td></tr>';
    });
    $('#wrongquestions').html(content);
}

function removeMistakeAnimate(pointer, index) {
    var elem = pointer.parent($('tr'));
    elem.slideUp('fast', function () {
        removeMistake(index);
    });
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

function getRandVersion() {
    var dateInSeconds = Math.round(new Date().getTime() / 1000).toString();
    return dateInSeconds + (Math.random() * 10000).toString();
}

function showMainMenu() {
    $('#files').show().removeClass('loading');
    $('#challenge, #voctable, #mistakes, #results').hide();
}