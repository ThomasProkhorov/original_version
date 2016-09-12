var RouterSharedCache = (function () {
    function RouterSharedCache(initState) {
        this.viewIn = null;
        this.filters = [];
        this.requestCase = null;
        this.productName = null;
        this.productShopID = null;
        this.backState = null;
        this.backHash = null;
        this.initFrom(initState);
    }
    RouterSharedCache.prototype.initFrom = function (other) {
        if (other) {
            for (var key in other) {
                if (this.hasOwnProperty(key) && other[key]) {
                    this[key] = other[key];
                }
            }
        }
        return this;
    };
    return RouterSharedCache;
})();
var CACHE_INSTANCE_NAME = "{D7EA3DD3-6B5A-412B-A966-C1175BF9E7E2}";
function setCache(data) {
    if (data) {
        window[CACHE_INSTANCE_NAME] = data;
    }
}
function getCache() {
    var instance = window[CACHE_INSTANCE_NAME];
    if (!instance) {
        instance = new RouterSharedCache();
        setCache(instance);
    }
    return instance;
}
var FiltrationHandlersFactory = (function () {
    function FiltrationHandlersFactory() {
    }
    FiltrationHandlersFactory.getPerparationHandler = function (requestingCase) {
        return FiltrationHandlersFactory._config[requestingCase].prepare;
    };
    FiltrationHandlersFactory.getResponseHandler = function (requestingCase) {
        return FiltrationHandlersFactory._config[requestingCase].handler;
    };
    FiltrationHandlersFactory.mergeSettings = function (requestCase, defaults, data) {
        var settings = FiltrationHandlersFactory._config[requestCase];
        // remove entries without data
        for (var key in data) {
            if (!data[key] || data[key].toString().trim() == "") {
                delete data[key];
            }
        }
        return $.extend(settings.requestSettings, defaults, {
            data: data
        });
    };
    FiltrationHandlersFactory._findAndMarkForSubcategory = function ($this) {
        $(".nav1 > a").each(function (el) {
            if ($(this).hasClass("orange-color")) {
                $(this).removeClass("orange-color");
                $(this).parents(".nav1:eq(0)").removeClass("currentcat");
            }
            if ($(this).children(":first-child").hasClass("grayscale-off")) {
                $(this).children(":first-child").removeClass("grayscale-off");
            }
        });
        $(".subCategory a").each(function () {
            if ($(this).hasClass("orange-color")) {
                $(this).removeClass("orange-color").css("font-weight", "normal");
                $(this).parents(".nav1:eq(0)").removeClass("currentcat");
            }
        });
        $this.parents("ul").siblings("a").addClass("orange-color").children(":first-child").addClass('grayscale-off');
        $this.children("a").addClass("orange-color").css("font-weight", "bold");
        $this.parents(".nav1:eq(0)").addClass("currentcat");
        $(".main_heading_h1").text($this.attr("data-name"));
    };
    FiltrationHandlersFactory._genericPreparationHandlerForCategory = function (requestParams) {
        var $this = $("li.nav1").filter(function (index, el) {
            return $(el).attr("data-id") == requestParams.categoryID;
        }).find("a.sf-with-ul");
        if ($this.length) {
            FiltrationHandlersFactory._findAndMarkForCategory($this);
        }
        else {
            $this = $(".subCategory").filter(function (index, el) {
                return $(el).attr("data-id") == requestParams.categoryID;
            });
            FiltrationHandlersFactory._findAndMarkForSubcategory($this);
        }
    };
    FiltrationHandlersFactory._findAndMarkForCategory = function ($this) {
        $(".nav1 > a").each(function (el) {
            if ($(this).hasClass("orange-color")) {
                $(this).removeClass("orange-color");
                $(this).parents(".nav1:eq(0)").removeClass("currentcat");
            }
            if ($(this).children(":first-child").hasClass("grayscale-off")) {
                $(this).children(":first-child").removeClass("grayscale-off");
            }
        });
        $this.children(":first-child").addClass('grayscale-off');
        $(".subCategory a").each(function () {
            if ($(this).hasClass("orange-color")) {
                $(this).removeClass("orange-color").css("font-weight", "normal");
                $(this).parents(".nav1:eq(0)").removeClass("currentcat");
            }
        });
        $this.addClass("orange-color");
        $this.parents(".nav1:eq(0)").addClass("currentcat");
        $(".main_heading_h1").text($this.parent().attr("data-name"));
    };
    FiltrationHandlersFactory.init = function () {
        FiltrationHandlersFactory._config[FiltrationHandlersFactory.cases.SINGLE] = {
            prepare: function (requestParams, data) {
                $("#searchProducts").val(requestParams.productName);
                delete data.categoryID;
            },
            handler: function (response, status, xhr) {
                var prodName = $("#searchProducts").val(), ct = xhr.getResponseHeader("content-type") || "";
                if (ct.indexOf("html") > -1) {
                    $(".main_heading_h1").html("תוצאות החיפוש: " + prodName);
                    $("#productResultWrapper ul").html(response);
                }
                if (ct.indexOf("json") > -1) {
                    response = JSON.parse(response);
                    if (response.status == "productNotFound") {
                        $("#productResultWrapper ul").html("<li class='paymnt_style_in'>" + response.localizationTextComponent + "</li>");
                        $(".main_heading_h1").html(response.localizationMessage);
                    }
                }
                animateToTop();
            },
            requestSettings: {
                type: "POST"
            }
        };
        FiltrationHandlersFactory._config[FiltrationHandlersFactory.cases.SMALL_TABLE] = {
            prepare: function (reqestParams, data) {
            },
            handler: function (html) {
                var $tableGalleryElement = $("#tableGallery"), maintHeadingText = $(".main_heading_h1").text();
                $tableGalleryElement.removeClass("grid_view_light").addClass("grid_view_dark");
                $tableGalleryElement.siblings("#listGallery").removeClass("list_view_dark").addClass("list_view_light");
                $("#productResultWrapper ul").html(html);
                $(".main_heading_h1").html(maintHeadingText);
            },
            requestSettings: {
                traditional: true,
                type: "GET"
            }
        };
        FiltrationHandlersFactory._config[FiltrationHandlersFactory.cases.CATEGORY] = {
            prepare: FiltrationHandlersFactory._genericPreparationHandlerForCategory,
            handler: function (html) {
                if (html == "") {
                    $("#indexProductFilters").empty();
                }
                $("#productResultWrapper ul").html(html);
            },
            requestSettings: {
                type: "POST",
                traditional: true
            }
        };
        FiltrationHandlersFactory._config[FiltrationHandlersFactory.cases.SUBCATEGORY] = {
            prepare: FiltrationHandlersFactory._genericPreparationHandlerForCategory,
            handler: function (html) {
                if (html == "") {
                    $("#indexProductFilters").empty();
                }
                $("#productResultWrapper ul").html(html);
            },
            requestSettings: {
                type: "POST",
                traditional: true
            }
        };
        FiltrationHandlersFactory._config[FiltrationHandlersFactory.cases.AS_GALLERY] = {
            prepare: function (reqestParams, data) {
            },
            handler: function (html) {
                var maintHeadingText = $(".main_heading_h1").text(), $listGalleryElement = $("#listGallery");
                $listGalleryElement.removeClass("list_view_light").addClass("list_view_dark");
                $listGalleryElement.siblings("#tableGallery").removeClass("grid_view_dark").addClass("grid_view_light");
                $("#productResultWrapper ul").html(html);
                $(".main_heading_h1").text(maintHeadingText);
            },
            requestSettings: {
                type: "GET",
                traditional: true
            }
        };
    };
    FiltrationHandlersFactory.cases = {
        SMALL_TABLE: "small_table",
        SINGLE: "single",
        CATEGORY: "category",
        SUBCATEGORY: "subcategory",
        AS_GALLERY: "as_gallery"
    };
    FiltrationHandlersFactory._config = {};
    return FiltrationHandlersFactory;
})();
function refreshPageTitle() {
    var $popup = $(".prdct_popup_h1"), titleText = $(".main_heading_h1").text();
    if ($popup.length) {
        titleText = $popup.text();
    }
    $("title").text(titleText);
    Shop.HideLoading($('.products_part'));
}
var app = Sammy(function () {
    FiltrationHandlersFactory.init();
    this.get("#!/firstCategoryProducts/:shopID", function (context) {
            console.log("firstCategoryProducts");
            var cacheInstance = getCache(),
            params = $.extend(context.params, cacheInstance),
            requestingCase = params.requestCase || "category",
            factory = FiltrationHandlersFactory,
            shopID = params.shopID;
            Shop.ShowLoading($('.products_part:eq(0)'));
            Shop.ShowLoading($('.products_part:last'));
            factory._findAndMarkForCategory($("#firstCategoryProducts"));

            $.ajax({
                type: "Get",
                dataType: "html",
                url: "/Shop/_GetFirstCategoryProducts",
                data: { shopID: shopID, favorite: true, allOrderedProducts: true, deals: true },
                cache: false,
                error: serverError,
            }).done(function (html) {
                $("#productResultWrapper ul").html(html);
            }).always(refreshPageTitle);

    });
    this.get("#!/favoriteProducts/:shopID", function (context) {
        console.log("favoriteProducts");
        var cacheInstance = getCache(),
            params = $.extend(context.params, cacheInstance),
            requestingCase = params.requestCase || "category",
            factory = FiltrationHandlersFactory,
            shopID = params.shopID;
            Shop.ShowLoading($('.products_part:eq(0)'));
            Shop.ShowLoading($('.products_part:last'));
            factory._findAndMarkForSubcategory($("#favoriteProducts").closest("li"));
            $.ajax({
                type: "Get",
                dataType: "html",
                url: "/Shop/_GetFirstCategoryProducts",
                data: { shopID: shopID, favorite: true },
                cache: false,
                error: serverError,
            }).done(function (html) {
                $("#productResultWrapper ul").html(html);
            }).always(refreshPageTitle);

    });

    this.get("#!/orderedProducts/:shopID", function (context) {
        console.log("orderedProducts");
        var cacheInstance = getCache(),
        params = $.extend(context.params, cacheInstance),
        requestingCase = params.requestCase || "category",
        factory = FiltrationHandlersFactory,
        shopID = params.shopID;
        Shop.ShowLoading($('.products_part:eq(0)'));
        Shop.ShowLoading($('.products_part:last'));
        factory._findAndMarkForSubcategory($("#orderedProducts").closest("li"));
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetFirstCategoryProducts",
            data: { shopID: shopID, allOrderedProducts: true },
            cache: false,
            error: serverError,
        }).done(function (html) {
            $("#productResultWrapper ul").html(html);
        }).always(refreshPageTitle);
    });

    this.get("#!/dealsProducts/:shopID", function (context) {
        console.log("dealsProducts");
        var cacheInstance = getCache(),
        params = $.extend(context.params, cacheInstance),
        requestingCase = params.requestCase || "category",
        factory = FiltrationHandlersFactory,
        shopID = params.shopID;
        Shop.ShowLoading($('.products_part:eq(0)'));
        Shop.ShowLoading($('.products_part:last'));
        factory._findAndMarkForSubcategory($("#dealsProducts").closest("li"));
         $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetFirstCategoryProducts",
            data: { shopID: shopID, deals: true },
            cache: false,
            error: serverError,
        }).done(function (html) {
            $("#productResultWrapper ul").html(html);
        }).always(refreshPageTitle);
    });

    this.get("#!/:productSku/:productName", function (context) {
        //var productShopId = $("#productSKU_" + context.params.productSku).data('productshopid');        
        var productShopId = getCache().productShopID || $("#productSKU_" + context.params.productSku).data('productshopid');
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetProductPopup",
            data: {
                productShopID: productShopId
            },
            cache: false,
            error: function () { alert('Server error'); },
        }).done(function (html) {
            var addthis_url = "http://s7.addthis.com/js/300/addthis_widget.js#pubid=ra-55059d817ee4f3c2";
            if (window["addthis"]) {
                window["addthis"] = null;
                window["_adr"] = null;
                window["_atc"] = null;
                window["_atd"] = null;
                window["_ate"] = null;
                window["_atr"] = null;
                window["_atw"] = null;
            }
            $.getScript(addthis_url, function (data, status, jqhr) {
                window["addthis"].init();
            });
            $("#productPopup").html(html).removeClass("dn");
            $("#light3").css("display", "block");
            $("#fade3").css("display", "block");
            animateToTop();
            refreshPageTitle();
            $(".popup_close1").click(function () {
                $("#productPopup").html("");
                var cache = getCache();
                if (cache.backState != null)
                    cache.backState.productShopID = cache.productShopID;
                setCache(cache.backState);
                var backUrl = cache.backHash || "#";
                if (backUrl) {
                    location.hash = backUrl;
                }
            });
        });
    });
    this.get("#!/product/:productShopID", function (context) {
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetProductPopup",
            data: {
                productShopID: context.params.productShopID
            },
            cache: false,
            error: function () { alert('Server error'); },
        }).done(function (html) {
            var addthis_url = "http://s7.addthis.com/js/300/addthis_widget.js#pubid=ra-55059d817ee4f3c2";
            if (window["addthis"]) {
                window["addthis"] = null;
                window["_adr"] = null;
                window["_atc"] = null;
                window["_atd"] = null;
                window["_ate"] = null;
                window["_atr"] = null;
                window["_atw"] = null;
            }
            $.getScript(addthis_url, function (data, status, jqhr) {
                window["addthis"].init();
            });
            $("#productPopup").html(html).removeClass("dn");
            $("#light3").css("display", "block");
            $("#fade3").css("display", "block");
            animateToTop();
            refreshPageTitle();
            $(".popup_close1").click(function () {
                $("#productPopup").html("");
                var cache = getCache();
                setCache(cache.backState);
                var backUrl = cache.backHash || "#";
                if (backUrl) {
                    location.hash = backUrl;
                }
            });
        });
    });
    this.get("#!/category/:shopID/:categoryID", function (context) {
        var cacheInstance = getCache(), params = $.extend(context.params, cacheInstance), filters = params.filters, filtersArray = filters ? filters.toString().split(",") : [], requestingCase = params.requestCase || "category", productName = !$('#searchProducts').val() ? '' : params.productName, factory = FiltrationHandlersFactory, 
        // mutual settings for all cases
        defaultAjaxRequestOptions = {
            dataType: "html",
            url: "/Shop/_GetProductByCategoryAndFilters",
            traditional: true,
            cache: false,
            error: function () { alert('Server error'); }
        }, data = {
            shopID: params.shopID,
            viewIn: params.viewIn,
            categoryID: params.categoryID,
            filters: filtersArray,
            productName: productName
        }, requestSettings = factory.mergeSettings(requestingCase, defaultAjaxRequestOptions, data), responseHandler = factory.getResponseHandler(requestingCase), preparationHandler = factory.getPerparationHandler(requestingCase);
        console.log(cacheInstance);
        var maintHeadingText = $(".main_heading_h1").text();
        console.log("current route GetProductByCategoryAndFilters. Args: ", [arguments, params, filters, filtersArray, requestingCase, factory, defaultAjaxRequestOptions, data, requestSettings, responseHandler]);
        //return false;
        preparationHandler(params, data);
        Shop.ShowLoading($('.products_part:eq(0)'));
        Shop.ShowLoading($('.products_part:last'));
        $.ajax(requestSettings)
            .done(responseHandler)
            .always(refreshPageTitle);
    });
    this.get("#", function () {
        console.log("root");
    });
});
function animateToTop() {
    $("body,html").animate({
        scrollTop: 0
    }, 100);
}
$(function () { return app.run(); });
