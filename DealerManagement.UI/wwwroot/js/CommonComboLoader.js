function LoadCombo(controlId, url, isDefaultRecordRequired = true) {

    $.ajax({
        url: url,
        data: {},
        type: 'get',
        async: false,
        cache: false,
        success: function (res) {
            var data = res;
            $("#" + controlId).empty();
            $("#" + controlId).get(0).options.length = 0;

            if (isDefaultRecordRequired) {
                $("#" + controlId).get(0).options[0] = new Option("----- Select -----", "xx");
            }
            if (data != null) {
                $.each(data, function (index, item) {
                    $("#" + controlId).get(0).options[$("#" + controlId).get(0).options.length] = new Option(item.value, item.name);
                });
            }

            // Add some responsive design styles
            //$("#" + controlId).addClass("form-control");
            //$("#" + controlId).addClass("select2Font");
            //$("#" + controlId).css({ "width": "100%" });
            //$("#" + controlId).css({ "width": "100%", "height": "calc(1.5em + 0.5rem + 2px)" });
            // data-selected
            var value = $("#" + controlId).attr("data-selected");
            var readonly = $("#" + controlId).attr("data-readonly");
            //$("#" + controlId).select2();
            if (value > 0) {
                $("#" + controlId).val(value).change();
            } else {
                $("#" + controlId).val(value);
            }

            if (readonly) {
                $("#" + controlId).attr('disabled', true);

            }


        },
        error: function () {
            ShowNotification('Error');
        }
    });
}

function LoadComboUsingClass(controlId, url, isDefaultRecordRequired = true) {

    $.ajax({
        url: url,
        data: {},
        type: 'get',
        async: false,
        cache: false,
        success: function (res) {
            var data = res;
            $("." + controlId).empty();
            $("." + controlId).get(0).options.length = 0;

            if (isDefaultRecordRequired) {
                $("." + controlId).get(0).options[0] = new Option("----- Select -----", "");
            }
            if (data != null) {
                $.each(data, function (index, item) {
                    $("." + controlId).get(0).options[$("." + controlId).get(0).options.length] = new Option(item.value, item.name);
                });
            }
            var value = $("." + controlId).attr("data-selected");
            var readonly = $("." + controlId).attr("data-readonly");
            if (value > 0) {
                $("." + controlId).val(value).change();
            } else {
                $("." + controlId).val(value);
            }

            if (readonly) {
                $("." + controlId).attr('disabled', true);

            }
        },
        error: function () {
            ShowNotification('Error');
        }
    });
}

function getBloodHound(url) {   
    return new Bloodhound({
        datumTokenizer: datum => Bloodhound.tokenizers.whitespace(datum.value),
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: {
            url: url +'?Prefix=%QUERY',
            // Map the remote source JSON array to a JavaScript object array
            filter: movies => $.map(movies, movie => ({
                value: movie.name,
            })),
            wildcard: '%QUERY',
        }
    });
}

function getTypeAheadConfig(url) {
    return {
        displayKey: 'value',
        source: getBloodHound(url).ttAdapter()
    }
}
