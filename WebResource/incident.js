 function onLoad(executionContext) {
    
	//    alert("js run on form!");

    let formContext = executionContext.getFormContext();   //get all context
	    // Add on change event to the typeCase field.
		let typeCase = formContext.getAttribute("new_type_case");
		typeCase.addOnChange(async function() {
	    await chengeOnFeildCaseType(formContext, typeCase);
		});
    }


async function chengeOnFeildCaseType(formContext, typeCase){

		let incidentTypeControl=formContext.getControl("new_type_case");
		incidentTypeControl.clearNotification();
		let codeIncidentType = typeCase.getValue();        
		let carId = getGuidOfLookupField(formContext,"new_car_of_case");
		if(carId) 
		{
			//if incident type not "other".
			if(codeIncidentType!==5){
				var result = await existIncidentForThisCarWithThisType(codeIncidentType,carId);
				var countReleventOpenIncident =result["@odata.count"];
				if(countReleventOpenIncident>0){
					incidentTypeControl.setNotification(`כבר פתוח אירוע תואם לרכב הזה בשם :${result.value[0].title}, 
					.לכן אי אפשר לפתוח אירוע נוסף
					`);
				}
			}
			if (codeIncidentType==3||codeIncidentType==4){
				try {
				  var jsonResultCar=await retrieveReleventFeildesFromCar(carId);
				  //console.log(jsonResultCar);
				 if(codeIncidentType==3){    
					 // test
					 let dateLisenceVadilty=jsonResultCar["new_date_lisence_vadilty"];
					 var result=testIsComingSoon(dateLisenceVadilty);
					 if(!result.isComingSoon)
					 {				
						incidentTypeControl.setNotification(`אין אפשרות לפתוח אירוע כי טרם הגיע זמן הטסט.
						 הטסט בעוד ${result.daysDifference} ימים.
						 `);
					 }  
 
				 }else{
					 // care
					 let kmNow=jsonResultCar["new_km"];
					 let nextCare=jsonResultCar["new_next_treament"];
					 var result =careIsComingSoon(kmNow,nextCare);
					 if(!result.isComingSoon){
						incidentTypeControl.setNotification(`אין אפשרות לפתוח אירוע כי טרם הגיע זמן הטיפול. הטיפול הבא בעוד 
						 ${result.differenceInKm} ק"מ.`);
					 }
				 }
			   }catch (error) {
				 console.log(error);
			   }
			 }
		}

	}

async function fetchModelRequest(fetchUrl){

		let fetchOptions = {
			method: "GET",
			headers: {
				"OData-MaxVersion": "4.0",
				"OData-Version": "4.0",
				"Content-Type": "application/json; charset=utf-8",
				"Accept": "application/json",
				"Prefer": "odata.include-annotations=*"
			}
		};
		try {
			let response = await fetch(fetchUrl, fetchOptions);
			let json = await response.json();
			if (response.ok) {
				return json;          
			} else {
				console.log(json.error.message);
				return null;
			}
		} catch (error) {
			console.log(error.message);
			return null;
		}
	
	
	}


//Retrieve from a car(lookup) entity the field: ("new_date_lisence_vadilty")
async function retrieveReleventFeildesFromCar(carId) {    

	let fetchUrl = `${Xrm.Utility.getGlobalContext().getClientUrl()}/api/data/v9.2/new_carses(${carId})?$select=new_next_treament,new_km,new_date_lisence_vadilty`;
	return await fetchModelRequest(fetchUrl);
}


async function existIncidentForThisCarWithThisType(codeIncidentType,carId){
	
	let fetchUrl = `${Xrm.Utility.getGlobalContext().getClientUrl()}/api/data/v9.2/incidents?$select=title&$filter=(new_type_case eq ${codeIncidentType} and _new_car_of_case_value eq ${carId})&$count=true`;
	return await fetchModelRequest(fetchUrl);
}


//get id object by feild(lookup)
function getGuidOfLookupField(formContext,nameFeild) {
    let guid = null;
    let feildLookup = formContext.getAttribute(nameFeild);
    if (feildLookup && feildLookup.getValue() && feildLookup.getValue()[0]) {
        guid = feildLookup.getValue()[0].id;
		guid =guid.replace(/[{}]/g, "");
    }
    return guid;
}



function careIsComingSoon(kmNow, nextCare) {
	let result = {
	  isComingSoon: nextCare - kmNow <= 3000,
	  differenceInKm: nextCare - kmNow
	};  
	return result;
  }


  function testIsComingSoon(dateLisenceVadilty) {
	var today = new Date();
	var licenseDate = new Date(dateLisenceVadilty); 
  
	//calculate the difference between the dates
	var diffInDays = Math.floor((licenseDate - today) / (1000 * 60 * 60 * 24));
  
	var result = {
	  isComingSoon: diffInDays <= 60,
	  daysDifference: diffInDays
	};
  
	return result;
  }


