$(function () {
    InitSelect2();

    //InitAccordions();

    InitDropDowns();
    InitRequired();

    InitDateRange();
    InitDateTime();    

    //InitDateTimePickers();
    //InitDateTimePickersChangeable();
    //formatPrice();
    //CheckBoxDesign();
    //trkPriceLenFormat();
    //selectFixTableRow();

});

$(document).ready(function () {

    $(":disabled, [readonly=readonly]").css("border-color", "orange").attr("title", "This field is readonly");
    rightAlignDecimalColumns(false);
    addClassNamesToAllTables();
    DecimalColumnsFormat();
    $(".fa-calendar-alt").addClass("cursorHand");

    $(".fa-calendar-alt").on("click", function () {
        
        var inputElement = $(this).parent().parent().parent().find("input.dateRange");
        inputElement.focus();
        inputElement.click();


    });

    $(".KendoTableCustomDesign").kendoGrid({
        sortable: true
    });

    $(".TwoDecimal").each(function (e) {
        this.value = parseFloat(this.value).toFixed(2);
    });


    $(".NumberCheck").change(function () {
        var val = $(this).val();
        var FormNumeric = FormNumericCheck();

        val = val.replace(/\,/g, '');
        if (isNaN(val)) {
            $(this).val('0');
            ShowResult("Fail", "Only numeric allowed!");
        }
        else if (val != "") {
           ;
            val = Number(parseFloat(val).toFixed(FormNumeric)).toLocaleString('en', { minimumFractionDigits: FormNumeric });
            $(this).val(val);
        }
    }); 

    $(".NumericCheck").change(function () {
        var val = $(this).val();
        var FormNumeric = FormNumericCheck();

        val = val.replace(/\,/g, '');
        if (isNaN(val)) {
            $(this).val('0');
            ShowResult("Fail", "Only numeric allowed!");
        }
        else if (val != "") {
           ;
            $(this).val(val);
        }
    }); 


    $('.btnApproval').on('click', function (e) {
        ShowNotification(2, 'Please go to approval panel!');
    });


});

function auditFieldDateTimeFormat(rawDate) {

    var formattedDate = '';
    if (rawDate && rawDate.trim() !== "")
    {
        var date = new Date(rawDate);

        var year = date.getFullYear();
        var month = String(date.getMonth() + 1).padStart(2, '0');
        var day = String(date.getDate()).padStart(2, '0');

        var hours = date.getHours();
        var minutes = String(date.getMinutes()).padStart(2, '0');
        var seconds = String(date.getSeconds()).padStart(2, '0');

        var ampm = hours >= 12 ? 'PM' : 'AM';
        hours = hours % 12;
        hours = hours ? hours : 12;

        formattedDate = `${year}-${month}-${day} ${hours}:${minutes}:${seconds} ${ampm}`;
    }    

    return formattedDate;
};

function encodeBase64(str) {
    return btoa(str || '');
}

// Decoding from Base64
function decodeBase64(str) {
    return atob(str || '');
}

$(function () {
    var inputs = $('.input-group input');
    var inputHeight = inputs.outerHeight();

    $(' textarea').outerHeight(inputHeight);
});

function InitDateRange() {
    $(".dateRange").daterangepicker({
        singleDatePicker: true,
        timePicker: false,
        locale: {
            format: 'Y-MM-DD'
        },
        showDropdowns: true,
        ranges: {
            'Today': [moment(), moment()],
        }
   
    });
};

function InitDateTime() {
    $(".dateTime").daterangepicker({
        singleDatePicker: true,
        timePicker: true,
        locale: {
            format: 'Y-MM-DD hh:mm A'
        }
    });
};

function InitDateTimePickers() {
    $('.dateRange').prop("readonly", "readonly");
    $(".dateRange").datepicker({
        changeYear: true, changeMonth: true, yearRange: '-90:+9', maxYear: '100',
        dateFormat: 'dd-M-yy'
    });
};
function InitDateTimePickersChangeable() {
    $(".datetimepicker-input").datepicker({
        changeYear: true, changeMonth: true, yearRange: '-90:+9', maxYear: '100',
        dateFormat: 'dd-M-yy'
    });
};





function InitSelect2() {
    $(".selectDropdown").select2();
    $(".selectDropdown").trigger('change');
    $(".selectDropdown").removeClass('form-control');
}

function checkDecimal() {
    var $this = $(this);
   ;
    $this.val($this.val().replace(/[^\d.]/g, ''));
}

function queryStringToObj(masterObj) {

    return JSON.parse('{"' + masterObj.replace(/&/g, '","').replace(/=/g, '":"') + '"}', function (key, value) { return key === "" ? value : decodeURIComponent(value) })
}


function parseForm(selector) {

    var parsedObject = {}

    $.each($(selector + " :input"), function () {
        var type = $(this).prop("type");
        var name = $(this).prop("name");

        if (type != "button") {

            if (name) {
                parsedObject[name] = $(this).val();

            }

        }
    })

    $.each($(selector + " select"), function () {

        var name = $(this).attr("data-name");
        parsedObject[name] = $("#" + $(this).prop("id") + " option:selected").text();


    })


    return parsedObject;
}



function parseFormModal(selector) {

    var parsedObject = {}

    $.each($(selector + " :input"), function () {
        var type = $(this).prop("type");
        var name = $(this).prop("name");

        var colName = name.replace("txtModal", '');

        if (type != "button") {

            if (name) {
                parsedObject[colName] = $(this).val();

            }

        }
    })

    $.each($(selector + " select"), function () {

        var name = $(this).attr("data-name");
        var colName = name.replace("txtModal", '');
        parsedObject[colName] = $("#" + $(this).prop("id") + " option:selected").text();
        parsedObject[colName + "ID"] = $("#" + $(this).prop("id") + " option:selected").val();

    })


    return parsedObject;
}

function goBack() {
    window.history.back();
}


function CopyItem(sender) {
    var tmpElement = $('<textarea style="opacity:0;"></textarea>');
    var parent = $(sender).closest('td').siblings('td').not(this).not(":hidden").each(function () {
        tmpElement.text(tmpElement.text() + $(this).find("input").val() + '\t');
    });

    tmpElement.appendTo($('body')).focus().select();
    document.execCommand("copy");
    tmpElement.remove();

    ShowResult("Info", "Data Copied to Clipboard!");

}

function CopyItemTableText(sender) {
    var tmpElement = $('<textarea style="opacity:0;"></textarea>');
    var parent = $(sender).siblings().not(this).not(":hidden").each(function () {
        tmpElement.text(tmpElement.text() + $(this).text() + '\t');
    });

    tmpElement.appendTo($('body')).focus().select();
    document.execCommand("copy");
    tmpElement.remove();

}



function priceLenFormat() {
    $('.priceLen:not([readonly])').click(function () {
        var value = parseFloat($(this).val().replace(/\,/g, ''));
        if (value == 0) {
            $(this).val("");
        }
    });
    $('.priceLen:not([readonly])').blur(function () {
        var val = $(this).val().replace(/\,/g, '');
        if (val == "" || isNaN(val)) {
            $(this).val("0");
        }
    });
    $(".NumberCheck").blur(function () {
        var val = $(this).val().replace(/\./g, '');
        val = val.replace(/\,/g, '');
        val = parseFloat(val);
        if (isNaN(val)) {
            $(this).val(0);
            ShowResult("Fail", "Only numeric allowed!");
        }
        NumberFormatCheck();
    });

}

function formatPrice() {

    $(".priceLen").each(function () {
        $(this).val(parseFloat($(this).val()).toFixed(4));
    });
}
function MyToggleBox(targetId, firstText, secondText) {
    if (firstText == null || firstText == "") {
        firstText = "%";
    }
    if (secondText == null || secondText == "") {
        secondText = "Q";
    }
    var status = $('#' + targetId).val();
    if (status == "True") {
        $('.' + targetId).addClass(" btn-blue");
        $('.' + targetId).text(firstText);
    }
    else {
        $('.' + targetId).text(secondText);
    }
    $('.' + targetId).click(function () {
        var x = $(this).hasClass("btn-blue");
        if (x == false) {
            $(this).addClass(" btn-blue");
            $(this).text(firstText);
            $('#' + targetId).val("True");
        }
        else {
            $(this).removeClass("btn-blue");
            $(this).text(secondText);
            $('#' + targetId).val("False");
        }
    });
}


function MyToggleBoxWithLabel(targetId, firstLabel, secondLabel) {
    ////var status = $('#IsPurchase').val() == 'Y' ? 'True' : 'False';
    var status = $('#' + targetId).val() == 'Y' ? 'True' : 'False';

    if (status == "True") {
        $('.' + targetId).addClass(" btn-green");
        $('.' + targetId).text(firstLabel);
    }
    else {
        $('.' + targetId).text(secondLabel);
    }
    $('.' + targetId).click(function () {
        var x = $(this).hasClass("btn-green");
        if (x == false) {
            $(this).addClass(" btn-green");
            $(this).text(firstLabel);
            $('#' + targetId).val("True");
        }
        else {
            $(this).removeClass("btn-green");
            $(this).text(secondLabel);
            $('#' + targetId).val("False");
        }
    });
}

function MyCheckBox(senderValue, targetId, outputId) {
    if (senderValue == "True" || senderValue == "Y") {
        $('#' + targetId).attr('checked', true);
    }
    $('#' + targetId).checkboxpicker({
        html: true,
        offLabel: '<span class="glyphicon glyphicon-remove NotActive">',
        onLabel: '<span class="glyphicon glyphicon-ok IsActive">'
    });
}


function CheckBoxDesign() {
    $('.NotActive').parent().click(function () {
        $(this).closest(".chkDesign").find(".chkValue").val("False");

        //$('.btn.active.btn-danger').css('background-color', '#c9302c!important');
    });
    $('.IsActive').parent().click(function () {
        $(this).closest(".chkDesign").find(".chkValue").val("True");

        ////$('#' + outputId).val("True");
        //$('.btn.active.btn-danger').css('background-color', '#449d44!important');

    });
}

function InitAccordions() {
    $(".Accordion").accordion({
        collapsible: true, active: true
    });
    $(".OpenAccordion").accordion({
        collapsible: false, active: 0, heightStyle: "content"
    });
    $(".CloseAccordion").accordion({
        collapsible: true, active: false, heightStyle: "content"
    });
}
function tablelength() {
    ////var len = [[5, 10, 25, 50, 100, 150, 200, 500 - 1], [5, 10, 25, 50, 100, 150, 200, 500, "All"]];
    var len = [[5, 10, 25, 50, 100, 150, 200, 500, 1000, 2000, 4000, 5000, 10000, 20000, 40000, 80000, 100000 - 1], [5, 10, 25, 50, 100, 150, 200, 500, 1000, 2000, 4000, 5000, 10000, 20000, 40000, 80000, 100000, "All"]];

    return len;
}
function CodeCheck(sender, T) {
    if ($(sender).val() == "") {
        return;
    }
    $(sender).parent().find('.NotMatch').remove();
    var url = "/Common/Common/EnumAlreadyExist?tableName=" + T + "&fieldName=Name&value=" + $(sender).val();
    $.ajax({
        type: "GET",
        url: url,
        error: function (xhr, status, error) {
            //
        },
        success: function (response) {
            if (!response) {
                $(sender).val('');
                if (!$(sender).parent().find('.NotMatch').hasClass('NotMatch')) {
                    $(sender).parent().append('<p class="NotMatch" style="color:red;" >Value is not Matched</p>');
                }
            }
        }
    });
}
function customToollip() {
    //$('.country').attr('title', 'write here or Double click for sample');
    $(' .department,.designation,.section,.country, .division,.district, .bloodGroup, .degree, .employmentStatus, .employmentType, .gender, .immigrationType, .leaveApproveStatus, .leaveType, .leftType, .meritalStatus, .nationality, .religion, .salaryPayMode, .salutation, .skillQuality, .trainingCourse, .trainingPlace, .trainingStatus, .travelType, .languageE, .languageCompetency, .languageFluency').attr('title', 'write here or Double click for sample');
    $(' .department,.designation,.section,.country, .division,.district, .bloodGroup, .degree, .employmentStatus, .employmentType, .gender, .immigrationType, .leaveApproveStatus, .leaveType, .leftType, .meritalStatus, .nationality, .religion, .salaryPayMode, .salutation, .skillQuality, .trainingCourse, .trainingPlace, .trainingStatus, .travelType, .languageE, .languageCompetency, .languageFluency').attr('placeholder', 'write here');
    $('.customDatePicker,.customTimePicker').attr('placeholder', 'click here');
}
function InitDateTimePickers1() {
    $('.customTimePicker')//.prop("readonly", "readonly");
    $('.customDatePicker').prop("readonly", "readonly");
    $(".customDatePicker").datepicker({
        changeYear: true, changeMonth: true, yearRange: '-90:+9', maxYear: '100',
        dateFormat: 'dd-M-yy'

    });
}
function InitDateTimePickersChangeable1() {
    $('.customTimePicker')//.prop("readonly", "readonly");
    //$('.customDatePicker').prop("readonly", "readonly");
    $(".customDatePickerChangeable").datepicker({
        changeYear: true, changeMonth: true, yearRange: '-90:+9', maxYear: '100',
        dateFormat: 'dd-M-yy'

    });
}
function InitDateTimePickersCurrent1() {
    //alert(1);
    $('.customTimePicker')//.prop("readonly", "readonly");
    $('.customDatePicker').prop("readonly", "readonly");
    $(".customDatePicker").datepicker({
        //changeYear: true, changeMonth: true, yearRange: '-90:+9', maxYear: '100',
        dateFormat: 'dd-M-yy',
        //minDate: new Date()

    });

    var date = $(".customDatePicker").val();
    //var today =new Date();// Session["SessionDate"];
    var today = FormatDate(new Date());
    //alert(today);
    if (date == "") {
        $(".customDatePicker").val(today);
    }
}

function InitDatePickers() {
    //var dt = FaizaSMS.WebUI.ViewModels.IdentityViewModel.SessionDate
    $(".DatePicker").prop("readonly", "readonly");
    $(".DatePicker:not(.OldDates)").datepicker({ changeYear: true, changeMonth: true, dateFormat: 'dd-M-yy', yearRange: '-100:+1' });
    $(".DatePicker.OldDates").datepicker({ changeYear: true, changeMonth: true, dateFormat: 'dd-M-yy', yearRange: '-100:+0', maxDate: '0', maxYear: '100' });
}

function FormatDate(input) {
    var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    var dt = new Date(input);
    return [dt.getDate(), months[dt.getMonth()], dt.getFullYear()].join('-');
}
function ParseDate(input) {
    var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    var parts = input.split('-');
    return new Date(parts[2], months.indexOf(parts[1]), parts[0]);
}

function valiDation(sender) {
    //$("textarea").attr('maxlength', '200');
    //$("input:text").attr('data-val', 'number');
    //$(sender).attr('data-val', 'number');

    $("#" + sender + " .required").parent().append('<p class="RequiredField" style="color:red;" >Value is Required</p>');
    $("#" + sender + " .RequiredField").hide();


    $(".required").change(function () {

        if ($(this).val() == "") {

            $(this).parent().find('.RequiredField').show();
        }
        else if ($(sender).attr('data-val-number')) {
            if (!parseInt($(this).val())) {
                $(this).parent().find('.RequiredField').show();
            }
            else {
                $(this).parent().find('.RequiredField').hide('slow');
            }
        }
        else {

            $(this).parent().find('.RequiredField').hide('slow');
        }
    });
    $(".NumberCheck").change(function () {
        var FormNumeric = FormNumericCheck();

        var val = $(this).val().replace(/\./g, '');
        val = val.replace(/\,/g, '');
        val = parseFloat(val);
        if (isNaN(val)) {
            $(this).val(0);
            ShowResult("Fail", "Only numeric allowed!");
        }
        if (val != "") {
            val = Number(parseFloat(val).toFixed(FormNumeric)).toLocaleString('en', { minimumFractionDigits: FormNumeric });
            $(this).val(val);
        }
    });

}

function requiredFields(sender) {
    var a = 0;
    for (var i = 0; i < $('#' + sender + ' .required').length; i++) {

        var object = $($('#' + sender + ' .required')[i]);
        var val = object.val();


        if (val == "" || val == null) {
            object.parent().find('.RequiredField').show();
            a++;
        }
    }
    return a;
}

function pageSubmit(sender) {
    var a = 0;


    var len = $('#' + sender + ' .required').length;

    for (var i = 0; i < $('#' + sender + ' .required').length; i++) {

        var object = $($('#' + sender + ' .required')[i]);
        var val = object.val();


        if (val == "" || val == null) {
            object.parent().find('.RequiredField').show();
            a++;
        }


        //////if ($($('#' + sender + ' .required')[i]).val() == "") {
        //////    $($('#' + sender + ' .required')[i]).parent().find('.RequiredField').show();
        //////    a++;
        //////}
    }
    if (a == 0) {
        //$("#" + sender).submit();
        $(".loading").show();
        var ab = $("#" + sender).submit();
        if (ab == "") {

            setTimeout(function () {
                $(".loading").fadeOut(500000).hide("slow");
            }, 10000);


        }
        else {
            setTimeout(function () {
                $(".loading").fadeOut(500000).hide("slow")
            }, 10000);
        }

    }
}

function BackToList(sender) {
    var url = $(sender).attr('data-url');
    window.location = url;
}
function NumberCheck(sender) {
   ;
    var val = $(sender).val().replace(/\./g, '');
    val = val.replace(/\,/g, '');
    if (isNaN(val)) {
        $(sender).val(0);
        ShowResult("Fail", "Only numeric allowed!");
    }
}

$(".TwoDecimal").each(function (e) {
    this.value = parseFloat(this.value).toFixed(2);
});

$(".TwoDecimalText").each(function () {
    this.innerText = parseFloat(this.innerText).toFixed(2);
});

$(".NoDecimal").each(function () {    
    this.value = parseFloat(this.value).toFixed(0);
});

$(".NumberCheckAddDetail").change(function () {
    var FormNumeric = FormNumericCheck();

    var val = $(this).val();

    var FormNumeric = FormNumericCheck();

    val = val.replace(/\,/g, '');

    if (isNaN(val)) {
        $(this).val(0);
        ShowResult("Fail", "Only numeric allowed!");
    }
    else {
        val = Number(parseFloat(val).toFixed(FormNumeric)).toLocaleString('en', { minimumFractionDigits: FormNumeric });
        $(this).val(val);
    }
});

$('.number').on('keypress', function (event) {
    var getVal = $('.number').val();
    var val = event.key;
    if (val == " ") {
        event.preventDefault();
    }
    else if (val == "Enter") {
        event.preventDefault();
    }
    else {
        if (isNaN(val)) {
            if (val == '+' || val == '-' || val == 'E' || val == 'e' || val == '.') {
                getVal = getVal + val.replace("-", "").replace("+", "").replace("E", "").replace("e", "").replace(".", "");
                $('.number').val(getVal);
                event.preventDefault();
            }
            else {
                ShowNotification(3, "Please input only numeric value.");
                event.preventDefault();
            }
        }
        else {
            console.log(val);
        }
    }
});

$(".NumberCheck").change(function () {
    var val = $(this).val();
    val = val.replace(/\,/g, '');
    if (isNaN(val)) {
        $(this).val(0);
        ShowResult("Fail", "Only numeric allowed!");
    }
    if (val != "") {
        var FormNumeric = FormNumericCheck();
        val = Number(parseFloat(val).toFixed(FormNumeric)).toLocaleString('en', { minimumFractionDigits: FormNumeric });
        $(this).val(val);
    }
});


function TimeFormat(sender) {

    //var time = $(sender).val();
    ////var regexp = new RegExp("/^\d+\.(?:[0-5]\d)$");
    //var regexp = new RegExp("/^\d+\.\d$");
    ////alert("m: " + $(sender).val().match("/^\d+\.(?:[0-5]\d)$"));

    ////alert(regexp);

    ////alert(time.search(regexp));

    ////var correct = (time.search(regexp) >= 0) ? true : false;
    //var correct = regexp.test(time);

    //alert(correct);

    //if (correct == false) {
    //    $(sender).val("0.00")
    //    ShowResult("Fail", time + " Invalid Time Format!");
    //}
}


$(".TimeFormat").change(function () {
    var regexp = "/^\d+\.(?:[0-5]\d)$";
    var correct = ($(this).val().search(regexp) >= 0) ? true : false;

    if (correct == false) {
        ShowResult("Fail", $(this).val() + " Invalid Time Format!");
    }
});

function pageSubmitJSON(sender) {
    var a = 0;
    for (var i = 0; i < $('#' + sender + ' .required').length; i++) {
        if ($($('#' + sender + ' .required')[i]).val() == "") {
            $($('#' + sender + ' .required')[i]).parent().find('.RequiredField').show();
            a++;
        }
    }
    return a;
}
function NumberFormatCheck() {
   ;
    $(".NumberCheck").each(function () {
        //console.log('hi1')
       ;
        var FormNumeric = FormNumericCheck();
        var val = ($(this).val()).replace(/\,/g, '');
        val = parseFloat(val);
        if (val != "") {
            val = Number(parseFloat(val).toFixed(FormNumeric)).toLocaleString('en', { minimumFractionDigits: FormNumeric });
            $(this).val(val);
        }
    });
}

function NumberFormatCheckText() {
    $(".NumberCheck").each(function () {
        //console.log('hi2')
       ;
        var FormNumeric = FormNumericCheck();
        var val = ($(this).text()).replace(/\,/g, '');
        val = parseFloat(val);
        if (val != "") {
            val = Number(parseFloat(val).toFixed(FormNumeric)).toLocaleString('en', { minimumFractionDigits: FormNumeric });

            $(this).text(val);
        }
    });
}
function FormNumericCheck() {
    var Numeric = $('#DecimalPlace').val();
    if (!Numeric) {
        Numeric = "2";
    }
    return Numeric;
}

function Division() {
    $("select.country").change(function () {
        var a = $(this);
        $(a).closest('div').parent().closest('div').parent('div').find('.district').html("<option></option>")
        var dropdownElements = a.closest('div').parent().next().find('.division');
        var url = a.closest('div').parent().next().find('.division').attr('data-url') + "?country=" + $(this).val();
        $.getJSON(url, function (item, textStatus, jqXHR) {
            var Listitems = '<option value="">Select</option>';
            $.each(item, function (i, data) {
                if (true) {

                }
                Listitems += "<option value='" + data.Value + "'>" + data.Text + "</option>";
            });
            dropdownElements.html(Listitems).addClass("DropdownInited");
        });
    });
}
function District() {
    $("select.division").change(function () {
        var a = $(this);
        var dropdownElements = a.closest('div').parent().next().find('.district');
        var url = a.closest('div').parent().next().find('.district').attr('data-url') + "?division=" + $(this).val();
        $.getJSON(url, function (item, textStatus, jqXHR) {
            var Listitems = '<option value="">Select</option>';
            $.each(item, function (i, data) {
                if (true) {

                }
                Listitems += "<option value='" + data.Value + "'>" + data.Text + "</option>";
            });
            dropdownElements.html(Listitems).addClass("DropdownInited");
        });
    });
}
function Country() {
    var county = $("select.country");
    $.each(county, function (a, b) {
        var selectedValue = $(b).attr('data-val');
        var url = $(b).attr('data-url');
        $.getJSON(url, function (item, textStatus, jqXHR) {
            var Listitems = '<option value="">Select</option>';
            $.each(item, function (i, data) {
                if (selectedValue == data.Value) {
                    Listitems += "<option selected='selected' value='" + data.Value + "'>" + data.Text + "</option>";
                    //
                    var selectedValue2 = $(b).closest('div').parent().next().find('.division').attr('data-val');
                    var url2 = $(b).closest('div').parent().next().find('.division').attr('data-url') + "?country=" + selectedValue;
                    $.getJSON(url2, function (item2, textStatus, jqXHR) {
                        var dListitems = '<option value="">Select</option>';
                        $.each(item2, function (i, division) {
                            if (selectedValue2 == division.Value) {
                                dListitems += "<option selected='selected' value='" + division.Value + "'>" + division.Text + "</option>";
                                //
                                var selectedValue3 = $(b).closest('div').parent().closest('div').parent('div').find('.district').attr('data-val');
                                var url3 = $(b).closest('div').parent().closest('div').parent('div').find('.district').attr('data-url') + "?division=" + selectedValue2;
                                $.getJSON(url3, function (item3, textStatus, jqXHR) {
                                    var dListitem3 = '<option value="">Select</option>';
                                    $.each(item3, function (i, disct) {
                                        if (selectedValue3 == disct.Value) {
                                            dListitem3 += "<option selected='selected' value='" + disct.Value + "'>" + disct.Text + "</option>";

                                        }
                                        else {
                                            dListitem3 += "<option value='" + disct.Value + "'>" + disct.Text + "</option>";
                                        }
                                    });
                                    $(b).closest('div').parent().closest('div').parent('div').find('.district').html(dListitem3).addClass("DropdownInited");
                                });
                                //
                            }
                            else {
                                dListitems += "<option value='" + division.Value + "'>" + division.Text + "</option>";
                            }
                        });
                        $(b).closest('div').parent().next().find('.division').html(dListitems).addClass("DropdownInited");
                    });
                }
                else {
                    Listitems += "<option value='" + data.Value + "'>" + data.Text + "</option>";
                }
            });
            $(b).html(Listitems).addClass("DropdownInited");
        });
    });
}

function InitDropDowns() {
    var dropdownElements = $('select.Dropdown:not(.DropdownInited)');
    $.each(dropdownElements, function (index, element) {
        var dropdownEl = $(element);
        var url = dropdownEl.attr('data-url');
        if (!url) {
            alert("no url");
            return;
        }

        var selected = dropdownEl.attr('data-selected');
        var dataCache = dropdownEl.attr('data-cache') ? true : false;

        //  dropdownEl.LoadingOverlay("show");

        $.ajax({
            url: url,
            type: 'GET',
            cache: dataCache,
            success: function (jsonData, textStatus, XMLHttpRequest) {
                var Listitems = '<option value="">Select</option>';

                $.each(jsonData, function (i, item) {

                    if (selected && selected == item.Value) {
                        Listitems += "<option selected='selected' value='" + item.Value + "'>" + item.Text + "</option>";
                    }
                    else {
                        Listitems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
                    }
                });
                dropdownEl.html(Listitems).addClass("DropdownInited");


                if (dropdownEl.hasClass('selectDropdown')) {
                    dropdownEl.trigger('change');
                }
            },

            complete: function () {
                //   dropdownEl.LoadingOverlay("hide");

            }
        });
    });
}

function InitCascadingDropDowns() {
    var dependentElements = $('select.Cascading:not(.DropdownInited)');
    $.each(dependentElements, function (index, element) {
        var dependentEl = $(element);
        var parentEl = $('#' + dependentEl.attr('data-parent')); // element.dataset.parent;
        var url = dependentEl.attr('data-url');
        var selected = dependentEl.attr('data-selected');
        var dataCache = dependentEl.attr('data-cache') ? true : false;
        var loadDropDownItems = function () {
            if (!parentEl.val()) {
                if (selected) {
                    setTimeout(loadDropDownItems, 300);
                }
                return;
            }
            $.ajax({
                url: url + parentEl.val(),
                type: 'GET',
                cache: dataCache,
                success: function (jsonData, textStatus, XMLHttpRequest) {
                    var Listitems = '<option></option>';
                    $.each(jsonData, function (i, item) {
                        if (selected && selected == item.Value) {
                            Listitems += "<option selected='selected' value='" + item.Value + "'>" + item.Text + "</option>";
                        }
                        else {
                            Listitems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
                        }
                    });
                    dependentEl.html(Listitems).addClass("DropdownInited");
                }
            });
        };
        parentEl.change(loadDropDownItems);
        if (selected) {
            loadDropDownItems();
        }
    });
}
function fnUpdate(submitBtn) {
    var $form = $(submitBtn).parents('form');
    var tt = "";
    $.ajax({
        type: "POST",
        url: $form.attr('action'),
        data: $form.serialize(),
        error: function (xhr, status, error) {
            "test"
        },
        success: function (response) {
            // $(".dialog-alert").dialog('open');
            $("#dialog-msg").append('' + response[1]);
            $(".ui-dialog").addClass('' + response[0]);

        }
    });
    return false;// if it's a link to prevent post
}
function InitRequired() {
    //$("[data-val-required]").css({
    //    $(".required").css({
    //    borderRight: "4px solid #f20"
    //});
}
var url = "";
function fromWriteColor(value) {
    $("." + value + " input:text,." + value + " textarea,." + value + " input:file,." + value + " select").addClass("wrightColor");
}
function fromReadColor(value) {
    $("." + value + " input:text,." + value + " textarea,." + value + " input:file,." + value + " select").addClass("readOnlyColor");
}
function fnReadOnly(value) {
    if ($("." + value).hasClass("readOnly")) {
        $("." + value).removeClass("readOnly");
        $("." + value + " input:text,." + value + " textarea").attr("readOnly", false);
        $("." + value + " input:file,." + value + " select,." + value + "  input:Checkbox,." + value + " .customDatePicker,." + value + " .customTimePicker").attr("disabled", false);
        $("." + value + " input:text,." + value + " textarea,." + value + " input:file,." + value + " select").addClass("wrightColor");
        $("." + value + " input:text,." + value + " textarea,." + value + " input:file,." + value + " select").removeClass("readOnlyColor");
        //$("." + value).attr("readOnly", false);
        //$("." + value).addClass("wrightColor");
        //$("." + value).removeClass("readOnlyColor");
        $("." + value + " .customTimePicker").attr('style', "cursor:pointer !important");

    }
    else {
        $("." + value + " input:file,." + value + " select,." + value + " input:Checkbox,." + value + " .customDatePicker,." + value + " .customTimePicker").attr("disabled", true);
        $("." + value).addClass("readOnly");
        $("." + value + " input:text,." + value + " textarea").attr("readOnly", true)
        $("." + value + " input:text,." + value + " textarea,." + value + " input:file,." + value + " select").removeClass("wrightColor");
        $("." + value + " input:text,." + value + " textarea,." + value + " input:file,." + value + " select").addClass("readOnlyColor");
        $("." + value + " .customTimePicker").attr('style', "cursor:not-allowed !important");
    }
    $('.customTimePicker').prop("readonly", "readonly");
    $('.customDatePicker').prop("readonly", "readonly");

}

function SelectAllForDelete() {
    $(".chkAll").on("click", function () {

        if ($(this).is(":checked")) {
            //alert(4986);
            $(this).closest("table").find("tbody input:checkbox").attr("checked", true);
        }
        else {
            $(this).closest("table").find("tbody input:checkbox").attr("checked", false);
        }
    });
    $(".paginate_enabled_next,.paginate_enabled_previous,.paginate_disabled_next,.paginate_disabled_previous").on("click", function () {
        $(this).parents().find("table th .chkAll").attr("checked", false);
    });
}


function deletedOne(sender) {
    var url = $(sender).attr("data-url") + "~";
    //alert(url);
    Ask("Are you sure to Delete!", function () {
        $.getJSON(url, function (item, textStatus, jqXHR) {
            if (result.indexOf("~") > 0) {
                ShowResult(result.split("~")[0], result.split("~")[1]);

                if (result.split("~")[0] == "Fail") {
                    return;
                }

            }
            else {
                ShowResult("Success", result);
            }

            setTimeout(function () {
                location.reload();
            }, 2000);
        });
    }, function () { });
}

function deletedData(sender, checkboxId, id) {
    var deletedIds = "";
    if (typeof id === 'undefined') {
        var length = $("#" + checkboxId + " tbody input:checkbox").length;
        for (var i = 0; i < length; i++) {
            if ($($("#" + checkboxId + " tbody input:checkbox")[i]).is(":checked")) {
                deletedIds += $($("#" + checkboxId + " tbody input:checkbox")[i]).attr("data-Id") + "~";
            }
        }
    }
    else {
        deletedIds = id + "~";
    }

    var url = $(sender).attr("data-url") + "?ids=" + deletedIds;
    if (deletedIds == "") {
        ShowResult("Fail", "Select first to Delete!");
        return;
    }
    Ask("Are you sure to Delete!", function () {
        $.getJSON(url, function (result, textStatus, jqXHR) {
            if (result.indexOf("~") > 0) {
                ShowResult(result.split("~")[0], result.split("~")[1]);

                if (result.split("~")[0] == "Fail") {
                    return;
                }

            }
            else {
                ShowResult("Success", result);
            }

            setTimeout(function () {
                location.reload();
            }, 2000);
        });
    }, function () { })
}
function journalData(sender, checkboxId, id) {
    var deletedIds = "";
    if (typeof id === 'undefined') {
        var length = $("#" + checkboxId + " tbody input:checkbox").length;
        for (var i = 0; i < length; i++) {
            if ($($("#" + checkboxId + " tbody input:checkbox")[i]).is(":checked")) {
                deletedIds += $($("#" + checkboxId + " tbody input:checkbox")[i]).attr("data-Id") + "~";
            }
        }
    }
    else {
        deletedIds = id + "~";
    }

    if (deletedIds == "") {
        ShowResult("Fail", "Select first to Journal!");
        return;
    }
    var tType = $(sender).attr("data-tType");
    var url = $(sender).attr("data-url") + "?ids=" + deletedIds + "&tType=" + tType;

    Ask("Are you sure to Journal!", function () {
        $.getJSON(url, function (result, textStatus, jqXHR) {
            if (result.indexOf("~") > 0) {
                //ShowResult(result.split("~")[0], result.split("~")[1]);

                if (result.split("~")[0] == "Fail") {
                    return;
                }
                else {
                    ShowResult("Success", "Journal Succesfully Created");
                }

            }
            else {
                ShowResult("Success", "Journal Succesfully Created");
            }
            setTimeout(function () {
                window.location = result;
                //location.reload();
            }, 2000);
        });
    }, function () { })
}

function postedData(sender, checkboxId, id) {
    var postedIds = "";
    if (typeof id === 'undefined') {
        var length = $("#" + checkboxId + " tbody input:checkbox").length;
        for (var i = 0; i < length; i++) {
            if ($($("#" + checkboxId + " tbody input:checkbox")[i]).is(":checked")) {
                postedIds += $($("#" + checkboxId + " tbody input:checkbox")[i]).attr("data-Id") + "~";
            }
        }
    }
    else {
        postedIds = id + "~";
    }

    var url = $(sender).attr("data-url") + "?ids=" + postedIds;
    if (postedIds == "") {
        ShowResult("Fail", "Select First to Post!");
        return;
    }
    Ask("Are you sure to Post!", function () {
        $.getJSON(url, function (result, textStatus, jqXHR) {
            if (result.indexOf("~") > 0) {
                ShowResult(result.split("~")[0], result.split("~")[1]);

                if (result.split("~")[0] == "Fail") {
                    return;
                }

            }
            else {
                ShowResult("Success", result);
            }

            setTimeout(function () {
                location.reload();
            }, 2000);
        });
    }, function () { })
}

function AuditData(sender, checkboxId, id) {
    operationMultiData(sender, checkboxId, "Audit", id);
}

function ApproveData(sender, checkboxId, id) {
    operationMultiData(sender, checkboxId, "Approve", id);
}

function RejectData(sender, checkboxId, id) {
    operationMultiData(sender, checkboxId, "Reject", id);
}

function ReceiveData(sender, checkboxId, id) {
    operationMultiData(sender, checkboxId, "Receive", id);
}

function operationMultiData(sender, checkboxId, operationName, id) {
    var ids = "";
    if (typeof id === 'undefined') {
        var length = $("#" + checkboxId + " tbody input:checkbox").length;
        for (var i = 0; i < length; i++) {
            if ($($("#" + checkboxId + " tbody input:checkbox")[i]).is(":checked")) {
                ids += $($("#" + checkboxId + " tbody input:checkbox")[i]).attr("data-Id") + "~";
            }
        }
    }
    else {
        ids = id + "~";
    }

    var url = $(sender).attr("data-url") + "?ids=" + ids;
    if (ids == "") {
        ShowResult("Fail", "Select First to " + operationName + "!");
        return;
    }
    Ask("Are you sure to " + operationName + "!", function () {
        $.getJSON(url, function (result, textStatus, jqXHR) {
            if (result.indexOf("~") > 0) {
                ShowResult(result.split("~")[0], result.split("~")[1]);

                if (result.split("~")[0] == "Fail") {
                    return;
                }

            }
            else {
                ShowResult("Success", result);
            }
            setTimeout(function () {
                location.reload();
            }, 2000);
        });
    }, function () { })
}


function NewTab(sender) {
    var url = $(sender).attr("data-url");
    var win = window.open(url, '_blank');
}

function GoTo(sender) {

    window.location = $(sender).attr("data-url");
}

function refreshDropdown(sender, targetId) {
    var targetEl = $('#' + targetId);
    var url = $(sender).attr("data-url");
    $.ajax({
        url: url,
        cache: false,
        beforeSend: function () { $(".loading").show(); },
        success: function (data) {
            var newDropdown = "<option value=''>Select</option>";
            $.each(data, function (i, state) {
                newDropdown += "<option value='" + state.Value + "'>" + state.Text + "</option>";
            });
            $(targetEl).html(newDropdown);
        },
        complete: function () { $(".loading").fadeOut(200).hide("slow") }

    });
}

function CheckAll(sender) {

    var unchecked = $(sender).closest("tbody").find('input:checkbox:not(":checked")').length;
    if (unchecked == 0) {
        $(sender).closest("table").find(" thead input:checkbox").attr('checked', true);
    }
    else {
        $(sender).closest("table").find(" thead input:checkbox").attr('checked', false);
    }
}
function CheckPromotionDate(sender) {
    var url = "/HRM/Home/CheckPromotionDate?employeeId=" + $("#promotionVM_EmployeeId").val() + "&date=" + $(sender).val();
    $.ajax({
        type: "GET",
        url: url,
        error: function (xhr, status, error) {
            //"test"
        },
        success: function (response) {
            if (!response) {
                $(sender).val('');
                ShowResult("Fail", "Promotion date can't be perior to join date/  last promotion date");
            }
        }
    });
}
function CheckTransferDate(sender) {
    var url = "/HRM/Home/CheckTransferDate?employeeId=" + $("#transferVM_EmployeeId").val() + "&date=" + $(sender).val();
    $.ajax({
        type: "GET",
        url: url,
        error: function (xhr, status, error) {
            //"test"
        },
        success: function (response) {
            if (!response) {
                $(sender).val('');
                ShowResult("Fail", "Transfer date can't be perior to join date/  last transfer  date");
            }
        }
    });
}
function ShowResult(status, msg, dataAction, dataUrl) {
   ;
    if (status == "Success") {
        toastr.success(msg, 'SSL ERP')
    }
    else if (status == "Fail") {
        toastr.error(msg, 'SSL ERP')
    }
    else if (status == "Info") {
        toastr.info(msg, 'SSL ERP')
    }
    else if (status == "Warning") {
        toastr.warning(msg, 'SSL ERP')
    }
    else {
        toastr.info(status, 'SSL ERP')
    }
}



function ShowResultProcess(status, msg, dataAction, dataUrl) {

    var html = '' +
        '<div class="modal fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">' +
        '<div class="modal-dialog" style="margin-top:150px;width:400px;">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<button type="button" class="close"  onclick="CloseModal(this)" aria-hidden="true">&times;</button>' +
        '<h2 class="modal-title" id="myModalLabel">SSL ERP</h2>' +
        '</div>' +
        '<div class="modal-body ' + status + '">' +
        msg +
        '</div>' +
        '<div class="modal-footer">' +
        '<input type="button" value="Ok" class="btn btn-info " onclick="CloseModal(this,\'' + dataAction + '\',\'' + dataUrl + '\')"/>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>';

    var dialogWindow = $(html).appendTo('body');
    dialogWindow.modal({ backdrop: 'static' });
}



function CloseModal(el, dataAction, dataUrl) {
    //if (dataAction == "formsubmit") {
    //    $("#" + dataUrl).submit();
    //}
    //else if (dataAction == "refreshparent") {
    //    window.parent.location = window.parent.location;
    //}
    //else if (dataAction == "refreshself") {
    //    window.location = window.location;
    //}
    //else if (dataAction == "redirect") {
    //    window.location = dataUrl;
    //}
    //else if (dataAction == "function") {
    //    eval(dataUrl);
    //}

    if (dataAction == "refreshparent") {
        window.parent.location = window.parent.location;
    }
    else if (dataAction == "redirect") {
        window.location = dataUrl;
    }
    var win = $(el).closest(".modal");
    win.modal("hide");
    setTimeout(function () {
        win.next(".modal-backdrop").remove();
        win.remove();
        $("body").removeClass("modal-open");
    }, 500);

}
function Ask(msg, yesCallback, noCallback) {
    var html = '' +
        '<div class="modal fade ask" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">' +
        '<div class="modal-dialog" style="margin-top:150px;width:400px;">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<button type="button" class="close" onclick="CloseModal(this);" aria-hidden="true">&times;</button>' +
        '<h2 class="modal-title" id="myModalLabel">SSL ERP</h2>' +
        '</div>' +
        '<div class="modal-body Success">' +
        msg +
        '</div>' +
        '<div class="modal-footer">' +
        '<input type="button" id="btnAskYes" value="Yes" class="btn btn-success" />' +
        '<input type="button" id="btnAskNo" value="No" class="btn btn-success" />' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>';

    var dialogWindow = $(html).appendTo('body');
    dialogWindow.modal({ backdrop: 'static' });

    $("#btnAskYes").on("click", function () {
        var win = $(this).closest(".modal");
        win.modal("hide");
        setTimeout(function () {
            win.next(".modal-backdrop").remove();
            win.remove();
            $("body").removeClass("modal-open");
        }, 500);
        setTimeout(yesCallback, 600);
    });
    $("#btnAskNo").on("click", function () {
        var win = $(this).closest(".modal");
        win.modal("hide");
        setTimeout(function () {
            win.next(".modal-backdrop").remove();
            win.remove();
            $("body").removeClass("modal-open");
        }, 500);
        setTimeout(noCallback, 600);
    });
}

function Okay(msg, yesCallback) {
    var html = '' +
        '<div class="modal fade ask" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">' +
        '<div class="modal-dialog" style="margin-top:150px;width:400px;">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<button type="button" class="close" onclick="CloseModal(this);" aria-hidden="true">&times;</button>' +
        '<h2 class="modal-title" id="myModalLabel">SSL ERP</h2>' +
        '</div>' +
        '<div class="modal-body Success">' +
        msg +
        '</div>' +
        '<div class="modal-footer">' +
        '<input type="button" onclick="OkayPressed(this,' + yesCallback + ');" value="Ok" class="btn btn-info" />' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>';

    var dialogWindow = $(html).appendTo('body');
    dialogWindow.modal({ backdrop: 'static' });
}

function OkayPressed(el, yesCallback) {
    var win = $(el).closest(".modal");
    win.modal("hide");
    setTimeout(function () {
        win.next(".modal-backdrop").remove();
        win.remove();
        $("body").removeClass("modal-open");
    }, 500);
    setTimeout(yesCallback, 600);
    console.log("from btn okay");
}


function InitButtons() {
    $('.AddRow:not(.AddRowInited)').on("click", function () {
        var url = $(this).attr('data-url');
        var container = $(this).attr('data-container');
        $.ajax({
            url: url,
            type: 'POST',
            cache: false,
            success: function (html) {
                $("#" + container).append(html);
            }
        });
        return false;
    }).addClass("AddRowInited");

    $('.RemoveRow:not(.RemoveRowInited)').on("click", function () {
        $(this).parents("div.row:first").remove();
        return false;
    }).addClass("RemoveRowInited");
}

function CheckImageSize(sender) {
    if (sender.files[0].size > (1024 * 100)) {
        ShowResult('Fail', 'Image size can be maximum 100kb');
        $(sender).val('');
    }
}
function CheckFileSize(sender) {
    if (sender.files[0].size > (1024 * 512)) {
        ShowResult('Fail', 'File size can be maximum 512kb');
        $(sender).val('');
    }
}
function FileDelete(sender) {
    Ask("Are you sure to delete this file!", function () {
        var url = "/Config/DropDown/FileDelete?filepath=" + $(sender).attr('data-url') + "&table=" + $(sender).attr('data-table') + "&field=" + $(sender).attr('data-field') + "&id=" + $(sender).attr('data-id');
        $.ajax({
            type: "GET",
            url: url,
            error: function (xhr, status, error) {
                //"test"
            },
            success: function (response) {
                if (response) {
                    $(sender).closest("div").parent().remove();
                    ShowResult("Success", "File deleted!");
                }
                else {
                    ShowResult("Fail", "File deleted failed!");
                }
            }
        })
    }, function () { })

}

function FixedCheck(sender) {
    if ($(sender).is(":checked")) {
        $(sender).closest('.row').find(".PortionSalaryTypeId option:selected").prop("selected", false);
        $(sender).closest('.row').find(".PortionSalaryTypeId").attr('disabled', true);
        $(sender).closest('.row').find("label.fr").text("Fixed")

    }
    else {
        $(sender).closest('.row').find(".PortionSalaryTypeId").attr('disabled', false);
        $(sender).closest('.row').find(".Portion").trigger('change');
        $(sender).closest('.row').find("label.fr").text("Rate(% of Basic)")

    }
}

function SalaryTypePortion(sender) {
    $(sender).closest('div').find('lable').hide();
    if (!isNaN($(sender).val())) {
        if (!$(sender).closest('.row').find(".IsFixed").is(":checked")) {
            if (parseFloat($(sender).val()) > 100) {
                $(sender).val("100.00");
            }
            else if (parseFloat($(sender).val()) < 0) {
                $(sender).val("0.00");
            }
        }

    }
    else {
        $(sender).val("0.00");
        $(sender).closest('div').append('<lable class="" style="color:red">Only numecir value is allowed!<lable>');

    }
}

function TaxPortion(sender) {
    if (!isNaN($(sender).val())) {

        //$(sender).parents("div.row:first").find("input.IsFixed");
        if (!$(sender).parents("div.first").find("input.IsFixed").is(":checked")) {
            //if (!$(sender).parents("div.row:first").find("input.IsFixed")) {
            alert("check");

            if (parseFloat($(sender).val()) > 100) {
                $(sender).val("100.00");
            }
            else if (parseFloat($(sender).val()) < 0) {
                $(sender).val("0.00");
            }
        }
        else {
            alert("not check");

        }
    }
    else {
        $(sender).val("0.00");
        $(sender).closest('div').append('<lable class="" style="color:red">Only numecir value is allowed!<lable>');

    }
}

function CheckNumeric(value) {
   ;
    var a = $(value).val();
    if (!$.isNumeric(a)) {
        $(value).val(0)
        ShowResult("Fail", "Please Enter Numeric data");

        return;
    }
}
$('.ccProject').click(function () {
    var departments = "";
    $('.department').html("");


    var oParam = { "projectId": "" };
    oParam.projectId = $('.ccProject').val();

    var url1 = "/Config/DropDown/DerparmentByProject";
    departments += "<option value='0'>Select</option>";

    $.getJSON(url1, oParam, function (data) {
        $.each(data, function (i, state) {
            departments += "<option value='" + state.value + "'>" + state.text + "</option>";
        });
        $('.ccDepartment').html(departments);
    });
});

function comparedate(first, second, firstName, secondName) {
    if (first > second) {
        ShowResult("Fail!", "" + firstName + " date can't be prior to " + secondName + " date");
        return false;
    }
}

function minmax(value, min, max) {
    $(value).attr({ type: "number", min: "0", step: "0.01" });
    var a = $(value).val();
    if (!$.isNumeric(a)) {
        $(value).val("")
        ShowResult("Fail", "Please Enter Numeric data");
        return;
    }
    else {
        if (parseInt(a) < min || isNaN(parseInt(a))) {
            ShowResult("Fail", "Please Enter more then")
            return;
        }
        else if (parseInt(a) > max) {
            ShowResult("Fail", "Please Enter greater then")
            return;
        }
        else return value;
    }
}
var submit = function (url, mydata) {
    var rrSuccess = false;
    $.ajax({
        type: 'POST',
        data: mydata, // #2
        url: url,
        beforeSend: function () { $(".loading").show(); },
        success: function (result) {
            rrSuccess = true;
            var msg1 = result.split('~')[0];
            var msg2 = result.split('~')[1];
            if (msg1 != "Fail") {

                ShowResult("Success", msg2);
                //location.reload();
                return rrSuccess;
            }
            else {
                ShowResult("Fail", msg2);
                return rrSuccess;
            }

        },
        complete: function () { $(".loading").fadeOut(200).hide("slow") }

    });
    return rrSuccess;

}


var submitProcess = function (url, mydata) {
    var rrSuccess = false;
    $.ajax({
        type: 'POST',
        data: mydata, // #2
        url: url,
        beforeSend: function () { $(".loading").show(); },
        success: function (result) {
            rrSuccess = true;
            var msg1 = result.split('~')[0];
            var msg2 = result.split('~')[1];
            if (msg1 != "Fail") {

                ShowResultProcess("Success", msg2);
                //location.reload();
                return rrSuccess;


            }
            else {
                ShowResultProcess("Fail", msg2);
                return rrSuccess;
            }

        },
        complete: function () { $(".loading").fadeOut(200).hide("slow") }

    });
    return rrSuccess;

}

$(document).keyup(function (e) {
    if (e.keyCode == 27) {
        $(".loading").fadeOut(200).hide("slow");
    }
});
function InitDropdownsCommon() {

    //alert(111);
    var fy = function () {
        $('select.fpDetailsCom').html("");
        var FiscalPeriodDetails = "";
        var fYear = $('.fiscalyearCom').val();

        var url = "/Config/DropDown/DropDownPeriodByFYear/?year=" + fYear;
        //FiscalPeriodDetailId += "<option value=0>Select</option>";

        $.getJSON(url, function (data) {
            $.each(data, function (i, state) {
                FiscalPeriodDetails += "<option value='" + state.Value + "'>" + state.Text + "</option>";
            });
            $('select.fpDetailsCom').html(FiscalPeriodDetails);
        });
    }
    function fyscalYearDetailToLoad() {
        $('select.fpDetailsComTo').html("");
        var fId = "";
        var fId = $('select.fpDetailsCom').val();
        var url1 = "/Config/DropDown/DropDownPeriodNext/?currentId=" + fId;
        CodeT = "<option value=0>Select</option>";
        $.getJSON(url1, function (data) {
            $.each(data, function (i, state) {
                CodeT += "<option value='" + state.Value + "'>" + state.Text + "</option>";
            });
            $('select.fpDetailsComTo').html(CodeT).change();;
        });
    }

    $('select.fpDetailsCom').click(function () {
        fyscalYearDetailToLoad();
    })
    //$('select.fpDetailsCom').change(function () {
    //    fyscalYearDetailToLoad();
    //})
    $('.fiscalyearCom').click(function () {
        fy();
    });

    $('.fiscalyearCom').change(function () {
        fy();
    });
    $('select.departmentsCom').click(function () {
        $('select.projectsCom').html("");
        $('select.sectionsCom').html("");
        var sections = "";
        var did = $('.departmentsCom').val();
        var url1 = "/Config/DropDown/SectionByDepartment/?departmentId=" + did;
        sections = "<option value=0>Select</option>";
        sections += "<option value=0_0>=ALL=</option>";
        $.getJSON(url1, function (data) {
            $.each(data, function (i, state) {
                sections += "<option value='" + state.Value + "'>" + state.Text + "</option>";
            });
            $('select.sectionsCom').html(sections).change();;
        });
    });
    $('select.sectionsCom').click(function () {
        $('select.projectsCom').html("");
        var projects = "";
        var sid = $('select.sectionsCom').val();
        var did = $('select.departmentsCom').val();
        var url1 = "/Config/DropDown/ProjectByDepartment/?departmentId=" + did + "&sectionId=" + sid;
        projects = "<option value=0>Select</option>";
        projects += "<option value=0_0>=ALL=</option>";
        $.getJSON(url1, function (data) {
            $.each(data, function (i, state) {
                projects += "<option value='" + state.Value + "'>" + state.Text + "</option>";
            });
            $('select.projectsCom').html(projects).change();;
        });
    });
    $('select.codeFCom').click(function () {
        $('select.codeTCom').html("");
        var CodeT = "";
        var CodeF = $('select.codeFCom').val();
        var url1 = "/Config/DropDown/EmployeeCodeNext/?currentCode=" + CodeF;
        CodeT = "<option value=0>Select</option>";
        CodeT += "<option value=0_0>=ALL=</option>";
        $.getJSON(url1, function (data) {
            $.each(data, function (i, state) {
                CodeT += "<option value='" + state.Value + "'>" + state.Text + "</option>";
            });
            $('select.codeTCom').html(CodeT).change();;
        });
    })

}

$(window).scroll(function () {
    if ($(this).scrollTop() > 50) {
        $('#back-to-top').fadeIn();
    } else {
        $('#back-to-top').fadeOut();
    }
});

$('#back-to-top').click(function () {
    $('body,html').animate({
        scrollTop: 0
    }, 800);
    return false;
});

$("#downClick").on("click", function () {
    scrolled = scrolled + 300;

    $(".cover").animate({
        scrollTop: scrolled
    });

});


function btnReceive(sender) {
    var IdOperation = $(sender).attr('data-Id');
    var Id = IdOperation.split('~')[0];
    var Operation = IdOperation.split('~')[1];
    if (Operation.toLowerCase() == 'true') {
        ShowResult("Fail", "Data Already Received!");
        return;
    }
    var url = $(sender).attr("data-url") + "?ids=" + Id + '~';
    var questionMSG = "Are you sure to Receive Data!";
    singleOperation(questionMSG, url);
}
function btnApprove(sender) {
    var IdOperation = $(sender).attr('data-Id');
    var Id = IdOperation.split('~')[0];
    var Operation = IdOperation.split('~')[1];
    if (Operation.toLowerCase() == 'true') {
        ShowResult("Fail", "Data Already Approved!");
        return;
    }
    var url = $(sender).attr("data-url") + "?ids=" + Id + '~';
    var questionMSG = "Are you sure to Approve Data!";
    singleOperation(questionMSG, url);
}
function btnPost(sender) {
    var IdPost = $(sender).attr('data-Id');
    var Id = IdPost.split('~')[0];
    var Post = IdPost.split('~')[1];
    if (Post.toLowerCase() == 'true' || Post.toLowerCase() == 'y') {
        ShowResult("Fail", "Data Already Posted!");
        return;
    }
    var url = $(sender).attr("data-url") + "?ids=" + Id + '~';
    var questionMSG = "Are you sure to Post Data!";
    singleOperation(questionMSG, url);
}

function btnPostId(sender) {
    var Id = $(sender).attr('data-Id');
    var Id = IdPost.split('~')[0];
    var url = $(sender).attr("data-url") + "?ids=" + Id + '~';
    var questionMSG = "Are you sure to Post Data!";
    singleOperation(questionMSG, url);
}

function btnPostNoSplit(sender) {
    var Id = $(sender).attr('data-Id');
    var Post = $(sender).attr('data-Post');
    if (Post == "Posted") {
        ShowResult("Fail", "Data Already Posted!");
        return;
    }
    var url = $(sender).attr("data-url") + "?ids=" + Id + '~';
    var questionMSG = "Are you sure to Post Data!";
    singleOperation(questionMSG, url);
}
function btnDelete(sender) {
    var IdPost = $(sender).attr('data-Id');
    var Id = IdPost.split('~')[0];
    var Post = IdPost.split('~')[1];
    if (Post.toLowerCase() == 'true') {
        ShowResult("Fail", "Data Already Posted! Can't Be Deleted!");
        return;
    }
    var url = $(sender).attr("data-url") + "?ids=" + Id + '~';
    var questionMSG = "Are you sure to Delete Data!";
    singleOperation(questionMSG, url);
}
function singleOperation(questionMSG, url) {
    Ask(questionMSG, function () {
        $.ajax({
            url: url,
            type: 'Post',
            beforeSend: function () { $(".loading").show(); },
            complete: function () { $(".loading").fadeOut(200).hide("slow") },
            success: function (result) {
                if (result.indexOf("~") > 0) {
                    ShowResult(result.split("~")[0], result.split("~")[1]);

                    if (result.split("~")[0] == "Fail") {
                        return;
                    }

                }
                else {
                    ShowResult("Success", result);
                }

                setTimeout(function () {
                    location.reload();
                }, 2000);
            }
        });
    });
}

function singlePostDlete(questionMSG, url) {
    Ask(questionMSG, function () {
        $.ajax({
            url: url,
            type: 'Post',
            beforeSend: function () { $(".loading").show(); },
            complete: function () { $(".loading").fadeOut(200).hide("slow") },
            success: function (result) {
                if (result.indexOf("~") > 0) {
                    ShowResult(result.split("~")[0], result.split("~")[1]);

                    if (result.split("~")[0] == "Fail") {
                        return;
                    }

                }
                else {
                    ShowResult("Success", result);
                }


                setTimeout(function () {
                    location.reload();
                }, 2000);

            }
        });
    });

}

function singleOperationGet(questionMSG, url) {
    Ask(questionMSG, function () {
        $.ajax({
            url: url,
            type: 'Get',
            beforeSend: function () { $(".loading").show(); },
            complete: function () { $(".loading").fadeOut(200).hide("slow") },
            success: function (result) {
                if (result.indexOf("~") > 0) {
                    ShowResult(result.split("~")[0], result.split("~")[1]);

                    if (result.split("~")[0] == "Fail") {
                        return;
                    }

                }
                else {
                    ShowResult("Success", result);
                }


                setTimeout(function () {
                    location.reload();
                }, 2000);
            }
        });
    });
}


function DropdownLoad($Dropdown, url, value) {
    $Dropdown.html("");



    ////$Dropdown.append($("<option />").val("").text("Select"));
    $Dropdown.select2("val", "");

    $.ajax({
        type: 'GET'
        , url: url
        , beforeSend: function () { $(".loading").show(); }
        , success: function (data) {
            $.each(data, function (i, state) {
                $Dropdown.append($("<option />").val(state.Value).text(state.Text));
            });
            if (value != "" && value != null && value != 0) {
                $Dropdown.select2("val", value);

                $Dropdown.val(value);

            }
        }
        , complete: function () { $(".loading").fadeOut(200).hide("slow") }

    });
}

function MsgAskOk(msg, status, noCallback) {

    //console.log(status);

    var color = "";

    if (status.toLowerCase() == "fail") {
        color = "red"
    }
    else {
        color = "#50bf50";
    }

    var html = '' +
        '<div class="modal fade ask" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">' +
        '<div class="modal-dialog" style="margin-top:150px;width:400px;">' +
        '<div class="modal-content">' +
        '<div class="modal-header">' +
        '<button type="button" class="close" onclick="CloseModal(this);" aria-hidden="true">&times;</button>' +
        '<h2 class="modal-title" id="myModalLabel">SSL ERP</h2>' +
        '</div>' +
        '<div class="modal-body Success" style="background-color: ' + color + ';color: white;">' +
        msg +
        '</div>' +
        '<div class="modal-footer">' +
        '<input type="button" id="btnAskOk" value="OK" class="btn btn-success" />' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>';

    //console.log(html);

    var dialogWindow = $(html).appendTo('body');
    dialogWindow.modal({ backdrop: 'static' });

    $("#btnAskOk").on("click", function () {
        var win = $(this).closest(".modal");
        win.modal("hide");
        setTimeout(function () {
            win.next(".modal-backdrop").remove();
            win.remove();
            $("body").removeClass("modal-open");
        }, 500);
        setTimeout(noCallback, 600);
    });



};


var approvalLevels = [];

var isWastageInPercent = [
    { key: '0', value: 'N' },
    { key: '1', value: 'Y' }
];

function generateApprovalLevels(upToValue) {
   ;
    if (upToValue !== undefined && upToValue !== "" && upToValue !== null) {
        upToValue = parseInt(upToValue);
    }
    for (var i = 1; i <= upToValue; i++) {
        approvalLevels.push({ key: i.toString(), value: i.toString() });
    }
};

function LoadApprovalLevelList($table) {   
    approvalLevels = [];
    $.ajax({
        type: "GET",
        url: "/Common/ApprovalLevelList",
        success: function (result) {
           ;            
            generateApprovalLevels(result[0].value);
           ;
            addRow($table);
        },
        error: function (result) {
           ;
            console.log(result);
        }
    });
};

function addRow($table) {
    
    var $row = $('<tr>');

    var serialNo = $table.find('tbody tr').length + 1; // Get the current row count + 1  

    $table.find('thead th').each(function () {
        var type = $(this).data('type');
        var search = $(this).data('search');
        var computed = $(this).data('computed');
        var isHidden = $(this).is(':hidden'); // check if the th element is hidden

        var columName = $(this).text();
        var columnNameAttr = $(this).attr('data-name');

        columName = columnNameAttr || columName;

        columName = columName.replace(/ /g, "-")

        var $cell = $('<td class=td-' + columName + '>');

        if (columName == 'SLNo') {
            
            $cell.append(serialNo); // Append the serial number cell to the row
        }

        if (isHidden) { // add hidden attribute to td element if th is hidden
            $cell.attr('hidden', true);
        }
        var val = '';
        if (type === 'decimal') {
            val = 0;
        }

        //// Create the dropdown for Approval Level

        if (type === 'checkbox') {
            var $checkbox = $('<input type="checkbox" />');
            $cell.append($checkbox);
        }

        if (columName === 'IsWastagePercent') {
            var $select = $('<select class="required form-control">');
            isWastageInPercent.forEach(function (level) {
                $select.append($('<option>', {
                    value: level.key,
                    text: level.value
                }));
            });

            $cell.append($select);
        }

        if (columName === 'ApprovalLevel') {
            var $select = $('<select class="required form-control">');
            approvalLevels.forEach(function (level) {
                $select.append($('<option>', {
                    value: level.key,
                    text: level.value
                }));
            });

            $cell.append($select);
        }

        $cell.appendTo($row);

    });


    $('<td>').html('<button type="button" class="btn btn-danger btn-sm remove-row-btn"><i class="fa fa-trash"></i></button>').appendTo($row);
    $row.appendTo($table.find('tbody'));

    // Focus on the first input field of the newly added row
    $row.find('input').first().focus();

    ;
    rightAlignDecimalColumns(true);
    defaultbranchAddinColumns();
};
function addRowx($table) {
    
    var $row = $('<tr>');

    var serialNo = $table.find('tbody tr').length + 1; // Get the current row count + 1  

    $table.find('thead th').each(function () {
        var type = $(this).data('type');
        var search = $(this).data('search');
        var computed = $(this).data('computed');
        var isHidden = $(this).is(':hidden'); // check if the th element is hidden

        var columName = $(this).text();
        var columnNameAttr = $(this).attr('data-name');

        columName = columnNameAttr || columName;
        columName = columName.replace(/ /g, "-");

        var $cell = $('<td class=td-' + columName + '>');

        if (columName == 'SLNo') {
            $cell.append(serialNo); // Append the serial number cell to the row
        }

        if (isHidden) { // add hidden attribute to td element if th is hidden
            $cell.attr('hidden', true);
        }

        var val = '';
        if (type === 'decimal') {
            val = 0;
        }

        // Creating dropdowns for specific columns like 'ApprovalLevel', 'IsWastagePercent'
        if (type === 'checkbox') {
            var $checkbox = $('<input type="checkbox" />');
            $cell.append($checkbox);
        }

        if (columName === 'IsWastagePercent') {
            var $select = $('<select class="required form-control">');
            isWastageInPercent.forEach(function (level) {
                $select.append($('<option>', {
                    value: level.key,
                    text: level.value
                }));
            });
            $cell.append($select);
        }

        if (columName === 'ApprovalLevel') {
            var $select = $('<select class="required form-control">');
            approvalLevels.forEach(function (level) {
                $select.append($('<option>', {
                    value: level.key,
                    text: level.value
                }));
            });
            $cell.append($select);
        }

        // Add the "Location" value explicitly here
        if (columName === 'Location') {
            var FromLocation = $("#FromLocation").val();
            if (!FromLocation) {
                FromLocation = $("#Location").val();
            }
            $cell.text(FromLocation); // Set the Location directly
        }

        // Set the "Unit of Measurement" to empty string ("")
        if (columName === 'Unit Of Measurment') {
            $cell.text(""); // Set the "Unit Of Measurment" to empty
        }

        $cell.appendTo($row);
    });

    $('<td>').html('<button type="button" class="btn btn-danger btn-sm remove-row-btn"><i class="fa fa-trash"></i></button>').appendTo($row);
    $row.appendTo($table.find('tbody'));

    // Focus on the first input field of the newly added row
    $row.find('input').first().focus();

    rightAlignDecimalColumns(true);
    defaultbranchAddinColumnsx();
};



function rightAlignDecimalColumns(DefultValueSet) {
    // Find all table headers with data-type="decimal"
    const decimalHeaders = $("th[data-type='decimal']");
    // Loop through each decimal header
    decimalHeaders.each(function () {
        // Find the index of the header cell in its row
        const index = $(this).index();

        // Loop through each row in the table body
        const rows = $("tbody tr");
        rows.each(function () {
            // Find the cell in the current row that corresponds to the header cell
            const cell = $(this).children().eq(index);

            // Right align the cell's text content
            cell.css("text-align", "right");
            cell.css("text-align", "right");

            if (cell.text().trim() != "0.00") {
                if (DefultValueSet && (cell.text().trim() === "" || cell.text() === null)) {
                    // Set the default value to "0.00"
                    cell.text("0.00");
                }
            }

            // Right align any input fields in the cell
            const inputFields = cell.find("input");
            inputFields.css("text-align", "right");
        });
    });
};

function defaultbranchAddinColumns() {
    // Find all table headers with data-type="decimal"
    const databranch = $("th[data-branch='default']");
    
    // Loop through each decimal header
    databranch.each(function () {
        
        // Find the index of the header cell in its row
        const index = $(this).index();

        // Loop through each row in the table body
        const rows = $("tbody tr");
        rows.each(function () {
            
            // Find the cell in the current row that corresponds to the header cell
            //const cell = $(this).children().eq(index);
            const cell = $(this).children().eq(index);

            // Right align the cell's text content
            cell.css("text-align", "left");
            cell.css("text-align", "left");

            if (cell.text().trim() != "" || cell.text() != null) {
                var FromLocation = $("#FromLocation").val();
                if (FromLocation === null || FromLocation === "" || typeof FromLocation === 'undefined') {
                    FromLocation = $("#Location").val();
                }
                cell.text(FromLocation);
            }
            SetToLocation();

            // Right align any input fields in the cell
            const inputFields = cell.find("input");
            inputFields.css("text-align", "right");
        });
    });
};

function defaultbranchAddinColumnsx() {
    // Find the column header with data-branch="default"
    const databranch = $("th[data-branch='default']");

    // Loop through each column with data-branch="default"
    databranch.each(function () {
        const index = $(this).index(); // Get the column index of the header

        // Loop through each row in the table body
        $("tbody tr").each(function () {
            const cell = $(this).children().eq(index); // Find the corresponding cell

            // Correctly align the text of decimal cells
            if (cell.text().trim() !== "") {
                var FromLocation = $("#FromLocation").val();
                if (!FromLocation) {
                    FromLocation = $("#Location").val();
                }
                if (index === 4) {  // Ensure we are updating the "Location" column (5th column in this case)
                    cell.text(FromLocation); // Set the Location text in the appropriate column
                }
            }

            // Right align any input fields if necessary
            const inputFields = cell.find("input");
            inputFields.css("text-align", "right");
        });
    });
};

function initEditTable($table, config) {

    $table.on('dblclick', 'td', function () {
        var $this = $(this);
        if ($this.find("button").length > 0) {
            return;
        }

        if ($this.find('input').length > 0) {
            return;
        }

        var computed = $this.closest('table').find('th').eq($this.index()).data('computed');

        if (computed) {
            return;
        }

        var type = $this.closest('table').find('th').eq($this.index()).data('type');

        var search = $this.closest('table').find('th').eq($this.index()).data('search');

        var columName = $this.closest('table').find('th').eq($this.index()).text();
        var columnNameAttr = $this.closest('table').find('th').eq($this.index()).attr('data-name');

        columName = columnNameAttr || columName;

        columName = columName.replace(/ /g, "_")

        var val = '';

        if (search) {

            var $inputGroup = $('<div>', {
                class: 'input-group'
            });

            var $input = $('<input>', {
                type: type,
                val: $this.text(),
                'data-type': type,
                readonly: true,
                class: 'form-control form-control-sm search  txt' + columName
            });

            $inputGroup.append($input);//.append('<div class="input-group-append"><div class= "input-group-text  ' + columName +'"> <i class="fa fa-search"></i></div ></div > ');

            $this.html($inputGroup);

            $input.focus();
            $input.select();

        }
        else {
          
                var $input = $('<input>', {
                    type: type,
                    val: $this.text(),
                    'data-type': type,
                    class: 'form-control form-control-sm txt' + columName
                });
            

            $this.html($input);
            $input.focus();
            $input.select();
        }

        rightAlignDecimalColumns(false);
    });

    //:not(.search)
    $table.on('blur', 'input', function (e) {

        var $this = $(this);
        var $td = $this.closest('td');

        var { val, error } = validateInput($this);

        var touched = $(this).data('touched');

        if (touched) {
            $this.focus();
            return;
        }

        if (error) {
            ShowNotification(3, error);
            $this.addClass('input-error');
            $this.focus();
            return;
        }

        handleAfterEdit($this);

    });

    function handleAfterEdit($this) {
        var $td = $this.closest('td');
        var type = $this.data('type');

        var val = $this.val();
        if (type === 'datetime-local') {
            val = new Date(val).toLocaleString();
        } else if (type === 'decimal') {
            var FormNumeric = FormNumericCheck();
            if (isNaN(parseFloat(val))){
                val="0"
            }
           ;
            //val = val.replace(',', '');
            val = val.replace(/\,/g, '');

            val = Number(parseFloat(val).toFixed(FormNumeric)).toLocaleString('en', { minimumFractionDigits: FormNumeric });

        }
       
        $td.html('<span data-type="' + type + '">' + val + '</span>');
    }

    $table.on('click', '.remove-row-btn', function () {

        $(this).closest('tr').remove();
        updateSerialNumbers();
    });


    function updateSerialNumbers() {
        $("#" + $table[0].id + " tbody tr").each(function (index) {
            $(this).find(".td-SLNo").text(index + 1);
        });        
    }

    function handleAfterEditWithVal($this) {
        var { val, error } = validateInput($this);

        if (error) {

            ShowNotification(3, error);
            $this.addClass('input-error');
            $this.focus();

            return;
        }

        handleAfterEdit($this)
    }

    function validateField($this) {
        var { val, error } = validateInput($this);

        if (error) {

            ShowNotification(3, error);
            $this.addClass('input-error');
            $this.focus();

        }

        return { val, error };

    }

    
    function validateInput($input) {
        var $td = $input.closest('td');
        var type = $input.data('type');
        var val = $input.val().trim();
        var $th = $table.find('th').eq($td.index());
        var isRequired = $th.data('required');
        var error = null;
        var columnIndex = $td.index();
        var group = $th.data('group');
        //console.log(group)

        var columnName = $th.text();

        // Custom validation for grouped columns
        if (group) {
            var $row = $input.closest('tr');
            var groupInputs = $row.find('td').map(function (index, td) {
                var $header = $table.find('th').eq(index);
                var tdVal = $(td).find('input').val();
                if (!tdVal) {
                    tdVal = $(td).text();
                }


                return $header.data('group') === group ? { columnName: $header.text(), value: tdVal.trim(), type: $header.data('type') } : null;
            }).get();

            var nonEmptyInputs = groupInputs.filter(function (input) {
                if (input !== null) {
                    if (input.type === 'decimal') {
                        return parseFloat(input.value == '' ? 0 : input.value) !== 0;
                    } else {
                        return input.value !== '';
                    }
                }
                return false;
            });

            if (nonEmptyInputs.length !== 1) {
                var failingColumns = groupInputs.map(function (input) {
                    return input !== null ? input.columnName : null;
                }).filter(function (columnName) {
                    return columnName !== null;
                }).join(', ');

                error = 'only one column should have a value. column names are: ' + failingColumns;
                return { val: val, error: error };
            }
        }

        if (isRequired && val === '') {

            error = 'This field is required: ' + columnName;
        }

        //if (type === 'decimal' && isNaN(parseFloat(val))) {
        //    return { val: 0.00, error: error };
        //}
        //if (type === 'decimal' && isNaN(parseFloat(val))) {
        //    error = 'Please enter a valid number for ' + columnName;
        //}

        return { val: val, error: error };
    }

    return {
        handleAfterEdit,
        handleAfterEditWithVal,
        validateField
    }
}

function serializeInputs(id) {
    var formArray = $('#' + id + ' :input').not('button').serializeArray();
    var formData = {};
    $.map(formArray, function (n, i) {
        formData[n['name']] = n['value'];
    });
    return formData;
}

function hasInputFieldInTableCells($table) {
    var tableCells = $table.find('td input:not([type="button"])');
    return tableCells.length > 0;
}

function hasLine($table) {
    var tableCells = $table.find('td');
    return tableCells.length > 0;
};


function serializeBomTable($table) {
    // Get the table headers (column names), excluding hidden columns
    const headers = $table.find('th').map((index, element) => {
        if ($(element).is(':hidden')) return null; // Exclude hidden columns

        var name = $(element).attr("data-name");
        return name ? name : $(element).text().trim();
    }).get().filter(header => header); // Filter out null values

    // Get the table rows (data)
    const rows = $table.find('tbody tr');

    // Convert the rows to an array of objects
    const data = rows.map((index, row) => {
        const cells = $(row).find('td').filter(':visible').map((index, cell) => $(cell).text().trim()).get(); // Only get visible cells
        return Object.fromEntries(headers.map((header, index) => [header.replace(/ /g, ""), cells[index]]));
    }).get();

    // Return the resulting array of objects
    return data;
}

function serializeTable($table) {
    // Get the table headers (column names)
    const headers = $table.find('th').map((index, element) => {
        var name = $(element).attr("data-name");
        if (name) return name;

        return $(element).text();

    }).get();

    // Get the table rows (data)
    const rows = $table.find('tbody tr');
   ;
    // Convert the rows to an array of objects
    const data = rows.map((index, row) => {
       ;
        const cells = $(row).find('td').map((index, cell) => $(cell).text()).get();
        return Object.fromEntries(headers.map((header, index) => [header.replace(/ /g, ""), cells[index]]));
    }).get();

    // Return the resulting array of objects
    return data;
};

function serializeTableData($table) {
   ;
    // Get the table headers (column names)
    const headers = $table.find('th').map((index, element) => {
        var name = $(element).attr("data-name");
        if (name) return name;

        return $(element).text();
    }).get();

    // Get the table rows (data)
    const rows = $table.find('tbody tr');

    // Convert the rows to an array of objects
    const data = rows.map((index, row) => {
        const cells = $(row).find('td').map((index, cell) => {
            // Check if the cell contains a form element
            const input = $(cell).find('input, select, textarea');
            if (input.length) {
                return input.val(); // Get the value of the form element
            } else {
                return $(cell).text(); // Get the text content of the cell
            }
        }).get();

        // Map headers to cell values
        return Object.fromEntries(headers.map((header, index) => [header, cells[index]]));
    }).get();

    // Return the resulting array of objects
    return data;
};

function getColumnSum(columnName, tableId) {
    let sum = 0;
    const table = document.getElementById(tableId);
    if (table) {
       ;
        const rows = table.rows;
        const headerRow = rows[0];
        const columnIndex = Array.from(headerRow.cells).findIndex(cell => cell.innerText.trim() === columnName);

        for (let i = 1; i < rows.length; i++) {
            const row = rows[i];
            const cellValue = parseFloat(row.cells[columnIndex].innerText.trim().replace(',', ''));

            if (!isNaN(cellValue)) {
                sum += cellValue;
            }
        }
    }


    return sum;
};

function getTbodyColumnSumAttr(dataName, tableId) {
   
    let sum = 0;
    const table = document.getElementById(tableId);
    if (table) {
        const headerRow = table.tHead ? table.tHead.rows[0] : table.rows[0];
        const columnIndex = Array.from(headerRow.cells).findIndex(cell => cell.getAttribute('data-name') === dataName);
        // loop only tbody rows
        const bodyRows = table.tBodies[0]?.rows || [];
        for (let i = 0; i < bodyRows.length; i++) {
            const row = bodyRows[i];
            let val = row.cells[columnIndex].innerText.trim();
            val = val.replace(/,/g, '');
            const cellValue = parseFloat(val);

            if (!isNaN(cellValue)) {
                sum += cellValue;
            }
        }
    }
     return sum;
};
function getColumnSumAttr(dataName, tableId) {
    let sum = 0;
    const table = document.getElementById(tableId);
    if (table) {
        const rows = table.rows;
        const headerRow = rows[0];
        const columnIndex = Array.from(headerRow.cells).findIndex(cell => cell.getAttribute('data-name') === dataName);

        for (let i = 1; i < rows.length; i++) {
            const row = rows[i];
           ;
            var val = row.cells[columnIndex].innerText.trim()
            val = val.replace(/\,/g, '');
            const cellValue = parseFloat(val.replace(',', ''));

            if (!isNaN(cellValue)) {
                sum += cellValue;
            }
        }
    }

    return sum;
};

function getRequiredFieldsCheckObj(obj, requiredFields, fieldMappings) {

    for (let i = 0; i < obj.length; i++) {
        for (let j = 0; j < requiredFields.length; j++) {
            const fieldName = requiredFields[j];
            const fieldValue = obj[i][requiredFields[j]].trim();
            const fieldDisplayName = fieldMappings[fieldName];
            if (fieldValue === '' || fieldValue === "0.00" || fieldValue === "0.0000" || fieldValue === "0.000000") {

                return fieldDisplayName + ' field is required';
            }
               
            }
        }

    return null; // return null if no errors are found
};

function getDebitCreditFieldsCheckObj(obj) {
    for (let i = 0; i < obj.length; i++) {
        const item = obj[i];
        if ((item.DebitAmount === null || parseFloat(item.DebitAmount) === 0 || item.DebitAmount ==="") &&
            (item.CreditAmount === null || parseFloat(item.CreditAmount) === 0 || item.CreditAmount === "")) {
            return 'Both Debit Amount and Credit Amount Cán not empty or 0 in Same Row';
        }
    }

    return null; // return null if no errors are found
};

function Confirmation(msg, callback) {
    bootbox.confirm(msg, function (result) {
        callback(result);
    });
};

function DecimalColumnsFormat() {
    // Get the decimal place value
    var DecimalPlace = $("#DecimalPlace").val();

    if (DecimalPlace === null || DecimalPlace === "" || typeof DecimalPlace === 'undefined') {
        DecimalPlace = "2";
    }

    // Find all elements with the class name "dFormat" for decimal value check
    $(".dFormat").each(function () {
        const cell = $(this);
        // Right align the cell's text content
        cell.css("text-align", "right");

        // Get the current text content, trim it, and format if it's not "0.00"
        if (cell.text().trim() != "0.00") {
            var result = cell.text().trim();
            var val = Number(parseFloat(result).toFixed(DecimalPlace)).toLocaleString('en', { minimumFractionDigits: DecimalPlace });
            // Set the formatted text back to the element
            cell.text(val);
        }
    });

    // Find all elements with the class name "dFormat3" for decimal value check
    $(".dFormat3").each(function () {
        DecimalPlace = "3";
        const cell = $(this);
        
        // Right align the cell's text content
        cell.css("text-align", "right");

        // Get the current text content, trim it, and format if it's not "0.00"
        if (cell.text().trim() != "0.00") {
            var result = cell.text().trim();
            var val = Number(parseFloat(result).toFixed(DecimalPlace)).toLocaleString('en', { minimumFractionDigits: DecimalPlace });
            // Set the formatted text back to the element
            cell.text(val);
        }
    });

    // Find all elements with the class name "dFormat4" for decimal value check
    $(".dFormat4").each(function () {
        DecimalPlace = "4";
        const cell = $(this);
        
        // Right align the cell's text content
        cell.css("text-align", "right");

        // Get the current text content, trim it, and format if it's not "0.00"
        if (cell.text().trim() != "0.00") {
            var result = cell.text().trim();
            var val = Number(parseFloat(result).toFixed(DecimalPlace)).toLocaleString('en', { minimumFractionDigits: DecimalPlace });
            // Set the formatted text back to the element
            cell.text(val);
        }
    });

};

function DecimalColumnsFormatbkp() {
    // Get the decimal place value
    var DecimalPlace = $("#DecimalPlace").val();

    if (DecimalPlace === null || DecimalPlace === "" || typeof DecimalPlace === 'undefined') {
        DecimalPlace = "2";
    }
    // Loop through each table in the document
    $("table").each(function () {
        // Find all table headers with data-type="decimal" within the current table
        const decimalHeaders = $(this).find("th[data-type='decimal']");
       
        // Loop through each decimal header
        decimalHeaders.each(function () {
            // Find the index of the header cell in its row
            const index = $(this).index();

            // Loop through each row in the table body of the current table
            const rows = $(this).closest("table").find("tbody tr");
            rows.each(function () {
                // Find the cell in the current row that corresponds to the header cell
                const cell = $(this).children().eq(index);

                // Right align the cell's text content
                cell.css("text-align", "right");

                if (cell.text().trim() != "0.00") {
                   
                    var result = cell.text().trim();
                    var val = Number(parseFloat(result).toFixed(DecimalPlace)).toLocaleString('en', { minimumFractionDigits: DecimalPlace });
                    cell.text(val);
                }

                // Right align any input fields in the cell
                const inputFields = cell.find("input");
                inputFields.css("text-align", "right");
            });
        });
    });
};

function SetToLocation() {
    // Find all table headers with data-type="decimal"
    const databranch = $("th[data-branch='selected']");

    // Loop through each decimal header
    databranch.each(function () {
        // Find the index of the header cell in its row
        const index = $(this).index();

        // Loop through each row in the table body
        const rows = $("tbody tr");
        rows.each(function () {
            // Find the cell in the current row that corresponds to the header cell
            const cell = $(this).children().eq(index);

            // Right align the cell's text content
            cell.css("text-align", "left");
            cell.css("text-align", "left");

    
                var ToLocation = $("#ToLocation").val();

                cell.text(ToLocation);


            // Right align any input fields in the cell
            const inputFields = cell.find("input");
            inputFields.css("text-align", "right");
        });
    });
};

function addClassNamesToAllTables() {
    $('table').each(function () {
        let table = $(this);

        // Get column names from table header
        let columnNames = [];
        table.find('th').each(function () {

            let columnNameAtt = $(this).data('name');
            let columnName = $(this).text();

            let finalName = columnNameAtt || columnName;       

            columnNames.push(finalName.replace(/ /g, "-"));

        });

        // Assign column names as classes to respective tds if not already present
        table.find('tbody tr').each(function () {
            let row = $(this);
            row.find('td').each(function (index) {
                let cell = $(this);
                let className = 'td-' + columnNames[index]  ;

                // Check if the cell does not have the class already
                if (!cell.hasClass(className)) {
                    cell.addClass(className);
                }
            });
        });
    });
};


