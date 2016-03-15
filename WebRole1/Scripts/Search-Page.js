function showDublinDropdown() {
    if ($("#counties-dropdown option:selected").text() == "Dublin") {
        $("#toggle-postcode-dropdown").show();
        $("#dates-dropdown1").hide();
        $("#dates-dropdown2").show();
    }
    else {
        $("#toggle-postcode-dropdown").hide();
        $("#dates-dropdown1").show();
        $("#dates-dropdown2").hide();
    }
}

function searchButtonOnDublinClick() {
    if ($("#counties-dropdown option:selected").text() == "Dublin")
    {
        $("#toggle-postcode-dropdown").show();
    }
    else {
        $("#toggle-postcode-dropdown").hide();
    }
}


