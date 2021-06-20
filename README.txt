League champion stat viewer - made by Michal Farkaš 492617

SETUP:
Before you use the program, you need to set your
database connection string in Config\connectionString.txt
and you need to get your riot API key by login into your 
riot account on https://developer.riotgames.com/ and 
generate the API key which lasts 24 hours.
after you have your API key, paste it into Config\riotApiKey.txt

HOW TO USE:
-  Input a player's summoner name into the summoner name field
-  Select the server on which the player is playing

-  Load All Matches button - loads all available matches
   this process can take up to 30 minutes if the player has played over 1000 games
   however, it usually takes only 3-10 minutes
-  Quick update button - loads the most recent 20 games played
-  Show stats button - calculate and display champion stats from matches played

-  After loading matches and pressing show stats button, the list on 
   the left side of screen should be populated by champions. You can sort this list
   by Games played, Winrate or KDA.
-  Export button will now be visible, you can export your currently displayed
   champion list stats as a CSV file

-  Selecting a champion will show more in-depth stats on the right side
   If the player has at least 3 games played on a certain champion, the program
   will automatically assign a grade from (S+, S, A, B, C, D) based on performace.
 
