/*  This script automatically replaces csv data generated by the error reporting feature.
    Usage: node main [json data]
    On windows, you have to wrap the json data in double quotes and escape the double quotes in the data with
    double double quotes, e.g.:

    node main "{""file"":""csv/Cars2.csv""}"
    instead of
    node main {"file":"csv/Cars2.csv"}
 */

var fs = require('fs');

console.log('------------------');
console.log('replacr v1');
console.log('');

var data = JSON.parse(process.argv[2]);
var file = data.file;
var fromCsv = data.fromCsv;
var toCsv = data.toCsv;

console.log('file: ' + file);
console.log('fromCsv: ' + fromCsv);
console.log('toCsv: ' + toCsv);

var path = '../' + file;
fs.readFile(path, 'utf8', function (err,data) {
    if (err) {
        console.error(err);
        process.exit(1);
    }

    if (data.indexOf(fromCsv) <= -1) {
        console.error('ERROR: string to replace was not found in source file.');
        process.exit(1);
    }
    var result = data.replace(fromCsv, toCsv);

    fs.writeFile(path, result, 'utf8', function (err) {
        if (err) {
            console.error(err);
            process.exit(1);
        }
        else {
            console.log('replace successful.');
        }
    });
});