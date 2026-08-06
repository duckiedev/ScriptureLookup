# ScriptureLookup
 Scripture Lookup application utilizing C#, AWS (Lambda, DynamoDB, SSM Parameter Store, IAM), and REST API.

 This function includes a passage query and verse of the day route.
 The verse of the day list is currently hardcoded.
 Each passage is sought from the DynamoDB cache first, and if it isn't found, it's pulled from the ESV Api and then stored.
 The API Key is stored in the SSM Parameter Store to keep it from being hardcoded into the application.

 Passage Query:
 <img width="1552" height="386" alt="image" src="https://github.com/user-attachments/assets/d355de5c-b925-4966-8992-cfe9abf74e5a" />
 
Verse-of-the-Day:
<img width="1552" height="386" alt="image" src="https://github.com/user-attachments/assets/ea22afd5-d45d-4966-b229-f7e6f868b5b0" />
