let express = require('express')        // express 모듈을 가져온다
let app = express();                    // espress 를 app 이름으로 정의하고 사용한다

app.get('/', function(req, res){
    res.send('Hello world');
});

app.get('/about', function(req, res){
    res.send('about data');
});

app.listen(3000, function(){
    console.log('listening on port 3000');
})