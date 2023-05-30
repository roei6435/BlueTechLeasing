// test the action from js.
var parameters = {};
parameters.businessNumber = "00000"; 
parameters.idNumber = "00000"; 
parameters.carNumber = "571500372"; 
parameters.caseType = "טיפול"; 
parameters.discriptionCase = "הנהג ליאור מורנו מצהיר על טיפול מתקרב ברכבו, סקודה אוקטביה, שבליסינג תפעולי עבור חברת קמביום"; // Edm.String

var req = new XMLHttpRequest();
req.open("POST", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/new_Create_Case", true);
req.setRequestHeader("OData-MaxVersion", "4.0");
req.setRequestHeader("OData-Version", "4.0");
req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
req.setRequestHeader("Accept", "application/json");
req.onreadystatechange = function () {
	if (this.readyState === 4) {
		req.onreadystatechange = null;
		if (this.status === 200 || this.status === 204) {
			var result = JSON.parse(this.response);
			console.log(result);
		} else {
			console.log(this.responseText);
		}
	}
};
req.send(JSON.stringify(parameters));
