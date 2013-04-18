
print("--- starting ticketing script ---")

manifestRequest = { FlightNumber = "ABC123", Date = "10/04/2012 09:45:00" }

print("Created manifest")
printTable(manifestRequest)

manifest = retrieveManifest(manifestRequest)

print("Called retrieveManifest")

print("Got manifest for flight: " .. manifest.FlightNumber)

for index,pnrNumber in pairs(manifest.PnrNumbers) do

	pnr = retrievePnr({ PnrNumber = pnrNumber })

	print("Got PNR: " .. pnr.PnrNumber .. " With email: " .. pnr.EmailAddress)

	renderResponse = render({ TemplateId = "my_lovely_template", Data = pnr })

	print("Got rendered output: " .. renderResponse.Content)

	emailRequest = 
	{ 
		ToAddress = pnr.EmailAddress, 
		FromAddress = "info@lovelyair.com", 
		Subject = "Your Ticket!",
		Body = renderResponse.Content
	}

	emailResponse = emailSend(emailRequest)

	print("Sent email to: " .. pnr.EmailAddress)

end

print("End of workflow")