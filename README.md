## Parse the Event-Signature header to get {signature}

## Using the HMACSHA256 algorithm and a special key, hash the model

## Compare the received hash with the one taken from {signature}

## If the hashes match, send the status 200, if not, 400
