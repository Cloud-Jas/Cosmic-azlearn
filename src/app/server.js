const express = require('express');
const { WebPubSubServiceClient } = require('@azure/web-pubsub');
const axios = require('axios').default;

const app = express();
let connectionString = process.argv[2] || process.env.WebPubSubConnectionString;
let endpoint = new WebPubSubServiceClient(connectionString, 'speechtransciptor');

app.get('/negotiate', async (req, res) => {
  let id = req.query.id || Math.random().toString(36).slice(2,7);
  let roles = req.query.id
    ? [`webpubsub.sendToGroup.${id}-control`, `webpubsub.joinLeaveGroup.${id}`]
    : [`webpubsub.sendToGroup.${id}`, `webpubsub.joinLeaveGroup.${id}-control`];
  let token = await endpoint.getClientAccessToken({ roles: roles });
  res.json({
    id: id,
    url: token.url
  });
});

app.get('/azmapstoken', async (req, res) => {

  res.setHeader('Content-Type', 'application/json');
  
  const headers = {
    headers: {
      'Referer':'https://cosmic-azpark.azurewebsites.net/'
    }
  };

  try {
    await axios.get('https://func-cosmic.azurewebsites.net/api/azureMapsToken',headers);    
  } catch (err) {
    console.log(err);
    res.status(401).send('There was an error authorizing your azure map');
  }

});

const cors=require("cors");
const corsOptions ={
   origin:'*', 
   credentials:true,             
   optionSuccessStatus:200,
}

app.use(cors())

app.get('/speech-token', async (req, res) => {

  res.setHeader('Content-Type', 'application/json');
  const speechKey = process.argv[3] || process.env.SPEECH_KEY;  
  const speechRegion = process.argv[4] ||  process.env.SPEECH_REGION;  

  const headers = {
    headers: {
      'Ocp-Apim-Subscription-Key': speechKey,
      'Content-Type': 'application/x-www-form-urlencoded'
    }
  };

  try {
    const tokenResponse = await axios.post(`https://${speechRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken`, null, headers);
    res.send({ token: tokenResponse.data, region: speechRegion });
  } catch (err) {
    console.log(err);
    res.status(401).send('There was an error authorizing your speech key.');
  }

});

app.use(express.static('public'));
app.listen(8080, () => console.log('app started'));