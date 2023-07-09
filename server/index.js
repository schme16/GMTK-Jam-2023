let port = 3080,
	express = require('express'),
	app = express(),
	{
		MongoClient,
		ObjectId,
		Server
	} = require('mongodb'),
	client = new MongoClient('mongodb://localhost'),
	router = new express.Router(),
	compression = require('compression'),
	cors = require('cors')


//Compress all the data before sending
app.use(compression({level: 9}))

//Add cross origin support
app.use(cors())

app.use('/post-score', (req, res) => {
	if (!!req.query.game && !!req.query.score && !!req.query.name) {
		let game = decodeURIComponent(req.query.game),
			score = parseFloat(decodeURIComponent(req.query.score)),
			name = decodeURIComponent(req.query.name)

		db.collection('data').insert({game, name, score})

		res.sendStatus(200)
	}
	else {
		res.sendStatus(412)
	}
})

app.use('/get-scores', (req, res) => {
	if (!!req.query.game) {
		let game = decodeURIComponent(req.query.game)
			limit = parseInt(decodeURIComponent((req.query.limit || 10)))
		db.collection('data').find({game}).project({_id:false}).sort({score: -1}).limit(limit).toArray((err, docs) => {
			res.json(docs)
		})
	}
	else {
		res.sendStatus(412)
	}
})

//Connect to the database
client.connect((err, database) => {

	//Shorthand the database
	db = client.db('scores')

	//Start the app listening
	app.listen(port, () => console.log(`App started on port:`, port))
})
	
	