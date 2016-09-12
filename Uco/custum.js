var ShopID;
var anotherScreenResolutionY = 865;
$(document).ready(function () {
   
    $("#shop-info-container .shop-info-heading").click(function () {
        var isChecked = !!$("#shop-info-container").prop("checked");
        if (isChecked) {
            $("#left_sec > #bannerWrapper").hide();
        } else
        {
            $("#left_sec > #bannerWrapper").show();
        }
        $("#shop-info-container").prop("checked", !isChecked)
    });

    $(".leftmenumodal").appendTo("body");

    $("#cart-button-container").appendTo("#head-cart-button-container");

    var topSpacing = 160;

    SearchProductData.shopID = $("#shopID").val();
    ShopID = $("#shopID").val();

    //////// open change shop popup
    var isOpenPopup = true;
    if (typeof (Storage) !== "undefined") {        
        isOpenPopup = localStorage.getItem("changeShopPopupButtonClick") !== 'false';
        localStorage.removeItem("changeShopPopupButtonClick");
    }
    if (!ShopID) {
        isOpenPopup = false;
    }
    if (location.href.toLowerCase().indexOf('shoppingcart/index/') > 0) {
        isOpenPopup = false;
    }
    if (isOpenPopup) {
        openChangeShopPopup();
    }    
   
    var checkoutFormValidator = $("#checkoutform").kendoValidator().data("kendoValidator");
    $("#checkoutFormSubmitBtn").on("click", function (event) {

        if (checkoutFormValidator.validate()) {

            //var shipTime = $('#ShipTime').val();
            //if (shipTime.length > 2) {
            //    shipTime += ":00";
            //}
            //$('#ShipTime').val(shipTime);

            document.getElementById('checkoutform').submit();
        }
        else {
        }
        $('.validation-summary-errors').html('');
        $('.k-invalid-msg').each(function (i, e) {
            $('.validation-summary-errors').append($(e).clone().wrap('<div>'));
        });

    });

    var formContactValidator = $("#formContact").kendoValidator().data("kendoValidator");
    $("#contactFormSubmitBtn").on("click", function (event) {
        if (formContactValidator.validate()) {
            document.getElementById('formContact').submit();
        }
        else {
        }
    });

    $("#shopType").kendoDropDownList({
        dataTextField: "Name",
        dataValueField: "ID",
        optionLabel: "בחר סוג חנות",
        dataSource: {
            transport: {
                read: {
                    dataType: "json",
                    url: "/Shop/_GetShopTypeList",
                }
            }
        },
        change: function (e) {
            var _address = getCookie("Address");
            var _latitude = getCookie("Latitude");
            var _longitude = getCookie("Longitude");
            var value = this.value();
            if (value) {
                if (!_latitude || !_longitude || !_address) {
                    $(location).attr('href', '/');
                    return;
                }
                var result = confirm($("#changeShopText").val());
                ga('send', 'event', 'Shop', 'changeShopType', value);
                if (result == true) {
                    $.ajax({
                        type: "Post",
                        url: "/Shop/LandingSelectShop",
                        data: { shopType: value, address: _address, Latitude: _latitude, Longitude: _longitude },
                        async: false,
                        error: serverError,
                        success: function (data) {
                            if (data == "/c/noshop") {
                                alert($("#noShopText").val());
                            }
                            else {
                                $(location).attr('href', data);
                            }
                        }
                    });

                } else {

                }
            }
        }
    });

    $("input.submit_ajax_button").removeAttr("disabled");

    if (screen.height < anotherScreenResolutionY) {
        topSpacing = 10;
        $(".header_main").css("position", "static");
        if ($(".header_main > div.nav").length > 0) {
            $(".midd_section").css("padding-top", "61px");
        }
        else {
            $(".midd_section2").css("padding-top", "28px");
        }
    }

    //item gallery show hide
    $(document.body).on("mouseenter", '.products_part ul li', function (e) {
      //  $(this).children("div.product_box_open").show();
    });

    $(document.body).on("mouseleave", ".products_part ul li", function (e) {
       // $("div.product_box_open").each(function () {
      //      $(this).hide();
       // });
    })

    $(document.body).on("mouseleave", "[name='ProductAttributeOptionID_box']", function (e) {
        e.stopPropagation();
    });//end    

    var footerH = $('footer');
    var fH = footerH.height();

    $('.fTab').on('click', function () {
        $(this).toggleClass('current');
        $('a:eq(0)', $(this)).toggleClass('opened');
        $('footer').slideToggle(500);
    });

    var win = $(window);
    var sizes = {
        half: 0.4,
        full: 1,
        threequarter: 3 / 4,
        onefive: 1.5,
        "double": 2,
        triple: 3
    }
    for (k in sizes) {
        var v = sizes[k]
        $("." + k).css({
            height: Math.floor(win.height() * v) + "px"
        });
    }

    $("#bannerWrapper1").sticky({
        topSpacing: topSpacing,
        wrapperClassName: "banner_sec_wrap",
        bottomSpacing: 150,
        getWidthFrom: "#bannerWrapper"
    });

    //$(".profile_sec").sticky({
    //    topSpacing: 250,
    //    wrapperClassName: "profile_sec_wrap",
    //    bottomSpacing: 350,
    //    getWidthFrom: ".profile_sec"
    //});

    $("#events_container").on("sticky_kit:stick", function (e) {
        $(e.target).html("got stick event");
    });

    $("#events_container").on("sticky_kit:unstick", function (e) {
        $(e.target).html("got unstick event");
    });

    $("#events_container").on("sticky_kit:bottom", function (e) {
        $(e.target).html("got bottom event");
    });

    $("#events_container").on("sticky_kit:unbottom", function (e) {
        $(e.target).html("got unbottom event");
    });

    $(document.body)
        .on("click", ".recalc", function () {
            console.log("Triggering recalc");
            $(document.body).trigger("sticky_kit:recalc")
        })
        .on("click", ".detach", function () {
            console.log("Triggering detach");
            $(".container .item").trigger("sticky_kit:detach");
        })
        .on("click", ".attach", function () {
            console.log("Triggering attach");
            attach();
        });

    $(document.body).on("click", ".grow_element", function (e) {
        var elm = $($(e.currentTarget).data("target"));
        elm.animate({ height: elm.height() * 1 }, function () {
            $(document.body).trigger("sticky_kit:recalc")
        });
    });

   
   
   // $('#shoping_list_box textarea').attr('placeholder', $('#shoping_list_box textarea').attr('placeholder') + ' שניצל עוף מעודן\n' + ' שניצל עוף דק ענק\n');
    $(".close-button").on("click", function () {
        $(this).parent("#shoping_list_box").addClass("dn");
    });

    $("#get").click(function () {
        alert('Thank you! Your Choice is:\n\nColor ID: ' + color.value() + ' and Size: ' + size.value());
    });
    var getAutoText = function () {

        var kac = $("#searchProducts").data("kendoAutoComplete");
        var text = '';
        if (kac) {
            text = kac.value();
        }
        return {
            text: text,
            shopID: ShopID,
        };
    }
    $("#searchProducts").kendoAutoComplete({
        filter: "contains",
        dataTextField: "Name",
        dataValueField: "ID",
        minLength: 3,
        dataSource: {
            type: "json",
            serverFiltering: true,

            transport: {
                read: {
                    url: "/Shop/_GetProductsForAutoComplete",
                    data: getAutoText,
                    dataType: "json", type: "POST"
                }
            }
        },
        change: function (e) {
            $("#searchProductBtn").trigger("click");
        },
        placeholder: "חיפוש מוצרים בחנות"
    }).keydown(function (e) {
        if (e.keyCode === 13) {
            $("#searchProductBtn").trigger("click");
        }
    });

    $("#headerUserName").keyup(function (e) {
        if (e.keyCode == 13) {
            $("#headerLogOnSubmit").click();
        }
    });

    $("#headrePassword").keyup(function (e) {
        if (e.keyCode == 13) {
            $("#headerLogOnSubmit").click();
        }
    });

    $(document.body).on("keyup", "#loginFormUserName", function (e) {
        if (e.keyCode == 13) {
            $("#loginFormSubmitBtn").click();
        }
    });

    $(document.body).on("keyup", "#loginFormPassword", function (e) {
        if (e.keyCode == 13) {
            $("#loginFormSubmitBtn").click();
        }
    });

    $("ul.sf-menu").superfish({
        pathClass: 'current-li',
        hoverClass: 'current',
    });

    $(".nav").on("mouseleave", function () {
        var r = $("a.orange-color");
        if (r.length > 0) {
            $("li.nav1").removeClass("current");
            $("li.nav1 ul").hide();
            //r.parent("li").addClass("current").end().siblings("ul").show(1);
        } else {
            $("li.nav1").removeClass("current");
            $("li.nav1 ul").hide();
        }
    });

    $("#clickme").click(function () {
        $("#book").toggle("slow", function () {
            // Animation complete.
        });
    });

    $("#shopInfo").on("click", function () {
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetShopInfo",
            data: { shopID: ShopID },
            cache: false,
            error: serverError,
        }).done(function (html) {
            var addthis_url = "http://s7.addthis.com/js/300/addthis_widget.js#pubid=ra-55059d817ee4f3c2";
            if (window.addthis) {
                window.addthis = null;
                window._adr = null;
                window._atc = null;
                window._atd = null;
                window._ate = null;
                window._atr = null;
                window._atw = null;
            }
            $.getScript(addthis_url, function (data, status, jqhr) {
                addthis.init();
            });
            $("#shopPopup").html(html).removeClass("dn");
            $("#light").css("display", "block");
            $("#fade").css("display", "block");
            animateToTop();
        });
    });

    $(".left_part4").on("click", function () {
        openChangeShopPopup();
    });

    $(document.body).on("click", ".productPopupChangeShop", function () {
        document.getElementById('light3').style.display = 'none';
        document.getElementById('fade3').style.display = 'none';
        animateToTop();
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_ChangeShopPopup",
            data: { shopID: ShopID },
            cache: false,
            error: serverError,
        }).done(function (html) {
            $("#changeShopPopup").html(html).removeClass("dn");
            $("#light4").css("display", "block");
            $("#fade4").css("display", "block");
        });
    });

    $("#firstCategoryProducts").on("click", function () {
        lazyLoadStop = true;
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetFirstCategoryProducts",
            data: { shopID: ShopID, favorite: true, allOrderedProducts: true, deals: true },
            cache: false,
            error: serverError,
        }).done(function (html) {
            $("#productResultWrapper ul").html(html);
        });
    });

    $("#favoriteProducts").on("click", function () {
        lazyLoadStop = true;
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetFirstCategoryProducts",
            data: { shopID: ShopID, favorite: true },
            cache: false,
            error: serverError,
        }).done(function (html) {
            $("#productResultWrapper ul").html(html);
        });
    });

    $("#orderedProducts").on("click", function () {
        lazyLoadStop = true;
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetFirstCategoryProducts",
            data: { shopID: ShopID, allOrderedProducts: true },
            cache: false,
            error: serverError,
        }).done(function (html) {
            $("#productResultWrapper ul").html(html);
        });
    });

    $("#dealsProducts").on("click", function () {
        lazyLoadStop = true;
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetFirstCategoryProducts",
            data: { shopID: ShopID, deals: true },
            cache: false,
            error: serverError,
        }).done(function (html) {
            $("#productResultWrapper ul").html(html);
        });
    });

    $("#btn_sms_popup").on("click", function () {
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetSMSPopup",
            cache: false,
            error: serverError,
        }).done(function (html) {
            $("#SMSPopup").html(html).removeClass("dn");
            $("#light2").css("display", "block");
            $("#fade").css("display", "block");
            animateToTop();
        });
    });

    var slugify = function (str) {
        var $slug = '';
        var trimmed = $.trim(str);
        $slug = trimmed.replace(/[^a-z0-9-]/gi, '-').
        replace(/-+/g, '-').
        replace(/^-|-$/g, '');
        return $slug.toLowerCase();
    }


    $(document.body).on("click", ".productPopupLink", function () {

        var
            $this = $(this),
            productSku = $this.data("productsku"),
           // productName = slugify($this.data("productname")),
            productshopid = $this.data("productshopid"),
            hash = "#!/" + productSku + "/" + productshopid,
            cacheInstance = new RouterSharedCache();

        cacheInstance.backState = getCache();
        cacheInstance.backHash = location.hash;
        cacheInstance.productShopID = $("#productSKU_" + productSku).data('productshopid');
        

        setCache(cacheInstance);

        hash = hash.replace("%", "");

        location.hash = hash;

        return false;
        //$.ajax({
        //    type: "Get",
        //    dataType: "html",
        //    url: "/Shop/_GetProductPopup",
        //    data: { productShopID: $this.attr("data-productshopid") },
        //    cache: false,
        //    error: serverError,
        //}).done(function (html) {

        //    var addthis_url = "http://s7.addthis.com/js/300/addthis_widget.js#pubid=ra-55059d817ee4f3c2";
        //    if (window.addthis) {
        //        window.addthis = null;
        //        window._adr = null;
        //        window._atc = null;
        //        window._atd = null;
        //        window._ate = null;
        //        window._atr = null;
        //        window._atw = null;
        //    }
        //    $.getScript(addthis_url, function (data, status, jqhr) {
        //        addthis.init();
        //    });
        //    $("#productPopup").html(html).removeClass("dn");

        //    $("#light3").css("display", "block");
        //    $("#fade3").css("display", "block");
        //    animateToTop();
        //});
    });

    $('#login_click').click(function () {
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Account/LogOnAjx",
            data: $('#login_form').serialize(),
            cache: false,
            error: function () { alert('Server error or youe is too much time enter wrong password, Try again later'); }
        }).done(function (data) {
            if (data.result == "ok") {
                location.reload();
            } else {
                //data.message - errors
                var errors = '';
                for (var m in data.message) {
                    for (var s in data.message[m].errors) {
                        errors += data.message[m].errors[s] + "\n";
                    }
                }
                alert(errors);
            }
        });
    });

    $("#login_signup_popup").on("click", function () {
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetLoginSignupPopup",
            cache: false,
            error: serverError,
        }).done(function (html) {
            $("#changeShopPopup").html(html).removeClass("dn");
            $("#light6").css("display", "block");
            $("#fade").css("display", "block");
        });
    });

    function constructRouteHash(requestCase, productName) {
        var
            newHref =
            "#!/category/" + SearchProductData.shopID
            + "/" + SearchProductData.categoryID
        // + "?_=" + new Date().valueOf()
        ;

        var cache = getCache();
        cache.initFrom({
            requestCase: requestCase,
            viewIn: (SearchProductData.viewIn || ""),
            filters: (SearchProductData.filters ? SearchProductData.filters.join(",") : ""),
            productName: (productName || "")
        });

        return newHref;
    }
    var
        knownRequestCases = {
            SMALL_TABLE: "small_table",
            SINGLE: "single",
            CATEGORY: "category",
            SUBCATEGORY: "subcategory",
            AS_GALLERY: "as_gallery"
        };

    //Products as Gallery + +
    $("#listGallery").on("click", function () {
        var maintHeadingText = $(".main_heading_h1").text();
        lazyLoadStop = false;
        $thiss = $(this);
        SearchProductData.viewIn = "table";

        navigateToVirtualRoute(knownRequestCases.AS_GALLERY);

        return false;

        //$.ajax({
        //    type: "Get",
        //    dataType: "html",
        //    url: "/Shop/_GetProductByCategoryAndFilters",
        //    data: { shopID: SearchProductData.shopID, viewIn: SearchProductData.viewIn, categoryID: SearchProductData.categoryID, filters: SearchProductData.filters },
        //    traditional: true,
        //    cache: false,
        //    complete: function () {

        //    },
        //    error: serverError,
        //}).done(function (html) {
        //    $thiss.removeClass("list_view_light").addClass("list_view_dark")
        //    $thiss.siblings("#tableGallery").removeClass("grid_view_dark").addClass("grid_view_light");
        //    $("#productResultWrapper").html(html);
        //    $(".main_heading_h1").text(maintHeadingText);

        //});
    });

    //Products as small table +
    $("#tableGallery").on("click", function () {
        var maintHeadingText = $(".main_heading_h1").text();
        lazyLoadStop = false;
        $thiss = $(this);
        SearchProductData.viewIn = "gallery";

        navigateToVirtualRoute(knownRequestCases.SMALL_TABLE);

        return false;

        //$.ajax({
        //    type: "Get",
        //    dataType: "html",
        //    url: "/Shop/_GetProductByCategoryAndFilters",
        //    data: {
        //        shopID: SearchProductData.shopID,
        //        viewIn: SearchProductData.viewIn,
        //        categoryID: SearchProductData.categoryID,
        //        filters: SearchProductData.filters
        //    },
        //    traditional: true,
        //    cache: false,
        //    error: serverError,
        //}).done(function (html) {
        //    $thiss.removeClass("grid_view_light").addClass("grid_view_dark")
        //    $thiss.siblings("#listGallery").removeClass("list_view_dark").addClass("list_view_light");
        //    $("#productResultWrapper").html(html);
        //    $(".main_heading_h1").html(maintHeadingText);
        //});
    });

    //single search btn click + +
   
    $("#searchProductBtn").on("click", function () {
        var prodName = $("#searchProducts").val();
        if (location.href.toLowerCase().indexOf('shoppingcart/index/') > 0)
        {
            var arr = location.pathname.split('/');
            
            var shopId = arr[arr.length - 1];
            location.href = '/Shop/Index/' + shopId + '#prodName=' + encodeURI(prodName);
            return false;
        }
       
        lazyLoadStop = true;

        navigateToVirtualRoute(knownRequestCases.SINGLE, null, prodName);

        return false;

        //$.ajax({
        //    type: "POST",
        //    url: "/Shop/_GetProductByCategoryAndFilters",
        //    data: {
        //        shopID: SearchProductData.shopID,
        //        productName: prodName,
        //        viewIn: SearchProductData.viewIn
        //    },
        //    cache: false,
        //    error: serverError,
        //}).done(function (response, status, xhr) {
        //    var ct = xhr.getResponseHeader("content-type") || "";
        //    if (ct.indexOf("html") > -1) {
        //        $(".main_heading_h1").html("תוצאות החיפוש: " + prodName);
        //        $("#productResultWrapper").html(response);
        //    }
        //    if (ct.indexOf("json") > -1) {
        //        if (response.status == "productNotFound") {
        //            $("#productResultWrapper").html("<div class='paymnt_style_in'>" + response.localizationTextComponent + "</div>");
        //            $(".main_heading_h1").html(response.localizationMessage);
        //        }
        //    }
        //    animateToTop();
        //});
    });

    //select category + +
    $("li.nav1 > a").on("click", function () {
        var $this = $(this);
        $("li.nav1 > a").attr("data-current-target", "false");

        $this.attr("data-current-target", "true");
        Shop.SearchData["keywords"] = '';
        //SearchProductData.filters = new Array();

        //$(".nav1 > a").each(function (el) {
        //    if ($(this).hasClass("orange-color")) {
        //        $(this).removeClass("orange-color");
        //    }
        //    if ($(this).children(":first-child").hasClass("grayscale-off")) {
        //        $(this).children(":first-child").removeClass("grayscale-off");
        //    }
        //});
        //$(this).children(":first-child").addClass('grayscale-off');
        //$(".subCategory a").each(function () {
        //    if ($(this).hasClass("orange-color")) {
        //        $(this).removeClass("orange-color").css("font-weight", "normal");
        //    }
        //});
        //$this.addClass("orange-color");
        //$(".main_heading_h1").text($this.parent().attr("data-name"));
        $('#searchProducts').val('');
        if (!$this.hasClass("first")) {
            lazyLoadStop = false;
            if (SearchProductData.viewIn == "tableItemPartial") SearchProductData.viewIn = "table";
            SearchProductData.categoryID = $this.parent().attr("data-id");
            $('#searchProducts').val('');
            SearchProductData.productName = '';
            navigateToVirtualRoute(knownRequestCases.CATEGORY, $this);

            return false;

            //$.ajax({
            //    type: "POST",
            //    dataType: "html",
            //    url: "/Shop/_GetProductByCategoryAndFilters",
            //    data: {
            //        shopID: SearchProductData.shopID,
            //        categoryID: SearchProductData.categoryID,
            //        filters: SearchProductData.filters,
            //        viewIn: SearchProductData.viewIn
            //    },
            //    traditional: true,
            //    cache: false,
            //    error: serverError,
            //}).done(function (html) {
            //    if (html == "") {
            //        $("#indexProductFilters").empty();
            //    }
            //    $("#productResultWrapper").html(html);
            //});
        }
    });

    function navigateToVirtualRoute(requestCase, thisReference, productName) {
        var newHref = constructRouteHash(requestCase, productName);
        if (thisReference) {
            $(thisReference).attr("href", location + newHref);
        }
        location.hash = newHref;
        $("body,html").animate({
            scrollTop: 0
        }, 300);
    }

    //select subCategory + +
    $(".subCategory").on("click", function (e) {
        var $this = $(this);
        $("li.nav1 ul").hide();
        Shop.SearchData["keywords"] = '';
        //$(".subCategory").attr("data-current-target", "false");
        //$this.attr("data-current-target", "true");

        //SearchProductData.filters = new Array();
        //$(".nav1 > a").each(function (el) {
        //    if ($(this).hasClass("orange-color"))
        //        $(this).removeClass("orange-color");
        //    if ($(this).children(":first-child").hasClass("grayscale-off")) {
        //        $(this).children(":first-child").removeClass("grayscale-off");
        //    }
        //});

        //$(".subCategory a").each(function () {
        //    if ($(this).hasClass("orange-color"))
        //        $(this).removeClass("orange-color").css("font-weight", "normal");
        //});

        //$this.parents("ul").siblings("a").addClass("orange-color").children(":first-child").addClass('grayscale-off');


        //$this.children("a").addClass("orange-color").css("font-weight", "bold");
        //$(".main_heading_h1").text($this.attr("data-name"));


        if (!$this.hasClass("first")) {

            lazyLoadStop = false;
            if (SearchProductData.viewIn == "tableItemPartial") SearchProductData.viewIn = "table";
            SearchProductData.categoryID = $this.attr("data-id")
            $('#searchProducts').val('');
            SearchProductData.productName = '';
            navigateToVirtualRoute(knownRequestCases.SUBCATEGORY, $this);

            return false;

            //$.ajax({
            //    type: "POST",
            //    dataType: "html",
            //    url: "/Shop/_GetProductByCategoryAndFilters",
            //    data: {
            //        shopID: SearchProductData.shopID,
            //        categoryID: SearchProductData.categoryID,
            //        filters: SearchProductData.filters,
            //        viewIn: SearchProductData.viewIn
            //    },
            //    traditional: true,
            //    cache: false,
            //    error: serverError,
            //}).done(function (html) {
            //    if (html == "") {
            //        $("#indexProductFilters").empty();
            //    }
            //    $("#productResultWrapper").html(html);
            //});
        }
    });

    var winNoteWindow = $("#noteWindow").kendoWindow({
        height: "150px",
        visible: false,
        draggable: false,
        resizable: false,
        modal: true,
        title: "הוסף הערה",
        activate: function () {
            $("#productNoteText").select().focus();
        },
        close: function () {
            noteWindowValidator.hideMessages();
        }
    });
    var dialog = $("#noteWindow").data("kendoWindow");
    $(document.body).on("click", ".showNoteWindow", function () {
        showNoteWindow($(this),dialog);
    });
    var shopCartNote = null;
    $(document.body).on("click", ".tab_width8.shAttributeDescription", function () {
        shopCartNote = $(this);
        showNoteWindow(shopCartNote, dialog);
    });

    var noteWindowValidator = $("#noteWindow").kendoValidator().data("kendoValidator");
    $(document.body).on("click", "#addProductNote", function () {
        $this = $(this);
        var id = $this.attr("data-productid");
        var text = $("#productNoteText").val();
        if (noteWindowValidator.validate()) {
            $.ajax({
                type: "POST",
                dataType: "html",
                url: "/Shop/_AddNoteForProduct",
                data: { productID: id, text: text },
                cache: false,
                error: serverError,
            }).done(function (html) {
                $(".noteForProduct[data-productid=" + id + "]").html(html);
                dialog.close();
                $("#productNoteText").val("");
                if (!!shopCartNote) {
                    shopCartNote.html(html);
                }
            });
        }
        else {
            //to do
        }
    });

    $(".contact_left_listing ul li").filter(':not(.current)').children("div").hide();

    $(".contact_left_listing ul li a").on("click", function (event) {
        var selfClick = $(this).siblings("div").is(':visible')
        if (!selfClick) {
            $(this).parents("ul").find("div:visible").slideToggle();
        }
        $(this).siblings("div").stop(true, true).slideToggle();

        $.each($(".contact_left_listing ul li a"), function (key, value) {
            if ($(this).siblings("div").is(":visible"))
                $(this).toggleClass("current");
        });
        event.preventDefault();
    });


    $("#back-top").hide();

    $(function () {
        //catch search product name
        if (location.hash.indexOf('prodName=') > 0) {
            var prodName = location.hash.replace('#prodName=', '');
            $("#searchProducts").val(decodeURI( prodName));
            $("#searchProductBtn").click();
        }
        $(window).scroll(function () {
            if ($(this).scrollTop() > 100) {
                $("#back-top").fadeIn();
            } else {
                $("#back-top").fadeOut();
            }

            if ($(window).scrollTop() + $(window).height() > $(document).height() - 280) {
                $("#back-top").addClass("back-top-absolute");
            } else {
                $("#back-top").removeClass("back-top-absolute");
            }
        });
        $("#back-top a").click(function () {
            $("body,html").animate({
                scrollTop: 0
            }, 300);
            return false;
        });
    });

   
        $(window).scroll(function () {
            if (screen.height >= anotherScreenResolutionY) {
                if ($(this).scrollTop() > 100) {
                    $(".nav_without_icons").hide();
                    //$(".nav1 > ul > li > a").hide();
                //    $(".topMenu > li img").hide();
                //    $('.topMenu > li div[style*="url("]').hide();
                //    $(".nav").css("height", "49px"); //91px
                //    $(".profile-status-line").css("height", "41px"); //height: 41px;
                //    $(".center_div > ul > li > a").css("padding-top", "8px"); //51px         
                //    $("li.nav1").hover(function () {
                //        $(".nav_without_icons").show();
                //        $(".nav1 > ul > li > a").show();
                //    },
                //    function () {
                //    });
                } else {
                    $(".nav_without_icons").show();
                   // $(".nav1 > ul > li > a").show();
                //    $(".topMenu > li img").show();
                //    $('.topMenu > li div[style*="url("]').show();
                //    $(".nav").css("height", "91px");
                //    $(".profile-status-line").css("height", "80px");
                //    $(".center_div > ul > li > a").css("padding-top", "51px");
                }
            }
        });
   
    //call first category
      //  if (!location.hash || location.hash.length < 5) {
       //     $('.nav1:eq(1) > a').click();
      //  }

});

var _gaq = _gaq || [];
_gaq.push(['_setAccount', 'UA-36251023-1']);
_gaq.push(['_setDomainName', 'jqueryscript.net']);
_gaq.push(['_trackPageview']);

(function () {
    var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
    ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
    var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
})();

var serverError = function () {
    //alert('Server error');
}

function toggle_visibility(id) {
    var e = document.getElementById(id);
    if (e.style.display == 'block') e.style.display = 'none';
    else e.style.display = 'block';

    hideAllBut(id);
}

function hideAllBut(id) {
    var lists = document.querySelectorAll('.list');
    for (var i = lists.length; i--;) {
        if (lists[i].id != id) {
            lists[i].style.display = 'none';
        }
    }
}
function animateToTop() {
    $("body,html").animate({
        scrollTop: 0
    }, 100);
}
var getCheckedFilterValue = function () {
    var msArray = new Array();

    if (SearchProductData.viewIn == "tableItemPartial") SearchProductData.viewIn = "table";

    $("#ms1 :selected").each(function (i, selected) {
        msArray.push($(selected).val());
    });
    $("#ms2 :selected").each(function (i, selected) {
        msArray.push($(selected).val());
    });
    $("#ms3 :selected").each(function (i, selected) {
        msArray.push($(selected).val());
    });
    $("#ms4 :selected").each(function (i, selected) {
        msArray.push($(selected).val());
    });
    $("#ms5 :selected").each(function (i, selected) {
        msArray.push($(selected).val());
    });
    SearchProductData.filters = msArray;
}

function getCookie(c_name) {
    if (document.cookie.length > 0) {
        c_start = document.cookie.indexOf(c_name + "=");
        if (c_start != -1) {
            c_start = c_start + c_name.length + 1;
            c_end = document.cookie.indexOf(";", c_start);
            if (c_end == -1) {
                c_end = document.cookie.length;
            }
            return unescape(document.cookie.substring(c_start, c_end));
        }
    }
    return "";
}

function scrollCheck() {
    var $right = $('#bannerWrapper'),
        width = $right.width()
    scrollTop = $(window).scrollTop(),
    windowHeight = $(window).height(),
    docHeight = $(document).height(),
    rightHeight = $right.height(),
    distanceToTop = rightHeight + 320 - windowHeight,
    distanceToBottom = scrollTop + windowHeight + 380 - docHeight;
    if (true || scrollTop > distanceToTop) {
       // console.log(scrollTop);
        $right.css({
            'position': 'absolute',
            'top': (
              //  scrollTop + windowHeight + 310 > docHeight ? distanceToBottom + 'px' :
                (scrollTop)+'px'),
            'width': width,
        });
    }
    else {
        $right.css({
            'position': 'relative',
            'bottom': 'auto'
        });
    }
}

//$(window).bind('scroll', scrollCheck);

function openChangeShopPopup()
{
    $.ajax({
        type: "Get",
        dataType: "html",
        url: "/Shop/_ChangeShopPopup",
        data: { shopID: ShopID },
        cache: false,
        error: serverError,
    }).done(function (html) {
        $("#changeShopPopup").html(html).removeClass("dn");
        $("#light4").css("display", "block");
        $("#fade4").css("display", "block");
        animateToTop();
        $(".po_bt .bt_p2").bind("click", function () {
            if (typeof (Storage) !== "undefined") {
                localStorage.setItem("changeShopPopupButtonClick", false);
            }
            
        });

    });

}

function showNoteWindow(s, dialog)
{
    var id = s.attr("data-productid");
    $.ajax({
        type: "GET",
        dataType: "html",
        url: "/Shop/GetUserNoteForProduct",
        data: { productID: id },
        cache: false,
        error: serverError,
        error: function () {
            console.log("error");
        }
    }).done(function (html) {
        $("#productNoteText").val(html);
        $("#addProductNote").attr("data-productid", id)
    });

    dialog.center();
    dialog.open();
}