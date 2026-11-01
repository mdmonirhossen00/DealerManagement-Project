
//#region Notification 
function MakePagination(tableId) {
	$('#' + tableId).dataTable({
		"paging": true,
		"bDestroy": true,
		"ordering": true,
		"info": true
		//"scrollX": true
	});
}
function MakePaginationWithoutOrder(tableId) {
	$('#' + tableId).dataTable({
		"paging": true,
		"bDestroy": true,
	    "bFilter":false,
		"ordering": false,
		"info": true,
		"scrollX": true
	});
}
function MakePaginationWithoutScrollX(tableId) {
	$('#' + tableId).dataTable({
		"paging": true,
		"bDestroy": true,
		"ordering": true,
		"info": true,
	});
}

function ShowNotification(type, message) {

	if (type != '0') {
	    if (type == '1' || type == 'success' || type == 'Success') {
	      
			toastr.success(message);
		}
	    else if (type == '2' || type == 'warning' || type == 'Warning') {
			toastr.warning(message);
		}
	    else if (type == '3' || type == 'error' || type == 'Error') {
			toastr.error(message);
		}
	    else if (type == '4' || type == 'info' || type == 'Info') {
		    toastr.info(message);
            
		}

	}
}


function GetServerDate() {
    var date = "";
    var url = '/Common/GetServerDate';
    $.ajax({
        url: url,
        method: 'get',
        dataType: 'json',
        async: false,
        data: {
        },
        success: function (data) {
            date = data;
        },
        error: function () {
        }
    });
    return date;
}

function GetMonthFirstDate() {
    
    var date = "";
    var url = '/Common/GetMonthFirstDate';
    $.ajax({
        url: url,
        method: 'get',
        dataType: 'json',
        async: false,
        data: {
        },
        success: function (data) {
            date = data;
        },
        error: function () {
        }
    });
    return date;
};

function GetMonthLastDate() {
    
    var date = "";
    var url = '/Common/GetMonthLastDate';
    $.ajax({
        url: url,
        method: 'get',
        dataType: 'json',
        async: false,
        data: {
        },
        success: function (data) {
            date = data;
        },
        error: function () {
        }
    });
    return date;
};



function GetFormatedDate(date) {
    var day = date.getDate();
    var month = date.getMonth() + 1;
    var year = date.getFullYear();
    return day + '-' + month + '-' + year;

}



//#region Validate Input
function decimal(e, ths) {
	var keynum;
	if (window.event) {
		keynum = e.keyCode;
	}
	else
		if (e.which) {
			keynum = e.which;
		}
	var tv = ths.value;
	if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
		if (e.which != 46 && e.which != 110 && e.which != 190) {
			ShowNotification(2, 'Decimal value only.');
			return false;
		}
		var charExists = (tv.indexOf('.') >= 0) ? false : true;
		if (!charExists) return false;
	}
}



function integer(e, ths) {
	var keynum;
	if (window.event) {
		keynum = e.keyCode;
	}
	else
		if (e.which) {
			keynum = e.which;
		}
	var tv = ths.value;
	if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
		if (e.which != 110 && e.which != 190) {
			ShowNotification(2, 'Integer value only.');
			return false;
		}
	}
}

function AjaxRequest(url, type, data, async, cache, successCallBack) {
	type == null ? 'post' : 'get',
    async == null ? true : false;
	cache == null ? false : true;
	$.ajax({
		url: url,
		type: type,
		data: data,
		async: async,
		cache: cache,
		success: successCallBack,
		error: function (xhr) {
		}
	});
};

function ShowLoaderGlobal() {
	var width = window.outerWidth;
	var height = window.outerHeight;
	var w = (width / 3) - 450;
	var h = (height / 3) - 100;

	var div = '<div id="ICustomLoader" style="height:' + height + 'px; width:' + width + 'px;background-color:rgba(0,0,0,0.5);z-index:999999;top:0;bottom:0;right:0;left:0; position:fixed;">' +
        '<div class="ILoader" style="margin-left:' + w + 'px; margin-top:' + h + 'px ;"><p style="color:rgb(236,2,140); font-size:20px;text-align: center;line-height: 30px;"><img  src="/Scripts/LoadingImages/loading4.gif" style="background-color:"/><img> Please Wait..</p></div>' +
        '</div>';
	$('body').append(div);

	$('#ICustomLoader').on("contextmenu", function (e) {
		e.preventDefault();
	});
}//rgba(0,0,0,.6)

function HideLoaderGlobal() {
	$('#ICustomLoader').remove();
}



function notifyAlert(style, message, icon, closeButton, msTimeout) {
    var options = {
        style: style,
        message: message,
        icon: icon,
        theme: null,
        timeout: msTimeout,
        close_button: closeButton
    };
    var n = new notify(options);
    n.show();
};


function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return (c.substring(name.length, c.length)) == "true";
        }
    }
    return "";
}

function menuChange() {
    if (getCookie("HasVersion")) {
        $("li[id='6004']").addClass("hidden");
        $("li[id='9033']").addClass("hidden");
        $("li[id='9034']").addClass("hidden");
    }
    else {
        $("li[id='6004']").removeClass("hidden");
        $("li[id='9033']").removeClass("hidden");
        $("li[id='9034']").removeClass("hidden");
    }
};

$(document).ready(function () {
    if (document.cookie.indexOf("HasVersion=") >= 0)
        setCookie("HasVersion", getCookie("HasVersion"), 30);
    else
        setCookie("HasVersion", true, 30);
    
    menuChange();
});

document.addEventListener("keydown", function (event) {
    if (event.altKey && event.ctrlKey && event.keyCode == 72) {
        setCookie("HasVersion", !getCookie("HasVersion"), 30);
        menuChange();
    }
});