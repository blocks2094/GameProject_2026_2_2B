// HTTP 모듈 로딩

let http = require("http");


http.createServer(function (require, response)
{
    response.writeHead(200, {'Content-Type' : 'text/plain'});

    response.end("Hello wolrd");
}).listen(8000);

console.log("Server running");