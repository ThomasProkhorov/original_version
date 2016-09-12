﻿/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/sammyjs/sammyjs.d.ts" />

declare var Shop: any;
interface IFiltrationRequestParams
{
    shopID: string;
    viewIn: string;
    categoryID: string;
    filters: string;
    productName: string;
    requestCase: string;
}

interface IFiltrationHandlersConfig
{
    requestSettings: JQueryAjaxSettings;
    prepare: (reqestParams: IFiltrationRequestParams, data: any) => any;
    handler: (response?, status?, xhr?) => any;
}

interface IRouterSharedCache
{
    viewIn: string;
    filters: number[];
    requestCase: string;
    productName: string;
    productShopID: number;

    backState: IRouterSharedCache;
    backHash: string;

    initFrom(other: IRouterSharedCache): IRouterSharedCache;
}

class RouterSharedCache implements IRouterSharedCache
{
    constructor(initState?: IRouterSharedCache)
    {
        this.viewIn = null;
        this.filters = [];
        this.requestCase = null;
        this.productName = null;
        this.productShopID = null;

        this.backState = null;
        this.backHash = null;

        this.initFrom(initState);
    }

    initFrom(other: IRouterSharedCache): IRouterSharedCache
    {
        if (other)
        {
            for (var key in other)
            {
                if ((<Object>this).hasOwnProperty(key) && other[key])
                {
                    this[key] = other[key];
                }
            }
        }


        return this;
    }

    viewIn: string;
    filters: number[];
    requestCase: string;
    productName: string;
    productShopID: number;

    backState: IRouterSharedCache;
    backHash: string;
}

var CACHE_INSTANCE_NAME = "{D7EA3DD3-6B5A-412B-A966-C1175BF9E7E2}";
function setCache(data: IRouterSharedCache)
{
    if (data)
    {
        window[CACHE_INSTANCE_NAME] = data;
    }
}

function getCache(): IRouterSharedCache
{
    var instance: IRouterSharedCache = window[CACHE_INSTANCE_NAME];
    if (!instance)
    {
        instance = new RouterSharedCache();

        setCache(instance);
    }

    return instance;
}

class FiltrationHandlersFactory
{
    static cases = {
        SMALL_TABLE: "small_table",
        SINGLE: "single",
        CATEGORY: "category",
        SUBCATEGORY: "subcategory",
        AS_GALLERY: "as_gallery"
    };

    public static _config: { [key: string]: IFiltrationHandlersConfig } = {};
    static getPerparationHandler(requestingCase: string): (params: IFiltrationRequestParams, data: any) => any
    {
        return FiltrationHandlersFactory._config[requestingCase].prepare;
    }
    static getResponseHandler(requestingCase: string): (response?, status?, xhr?) => any
    {
        return FiltrationHandlersFactory._config[requestingCase].handler;
    }
    static mergeSettings(requestCase: string, defaults: JQueryAjaxSettings, data: any): JQueryAjaxSettings
    {
        var settings = FiltrationHandlersFactory._config[requestCase];

        // remove entries without data
        for (var key in data)
        {
            if (!data[key] || data[key].toString().trim() == "")
            {
                delete data[key];
            }
        }

        return $.extend(settings.requestSettings, defaults, {
            data: data
        });
    }
    public static _findAndMarkForSubcategory($this: JQuery)
    {
        $(".nav1 > a").each(function (el)
        {
            if ($(this).hasClass("orange-color")) {
            $(this).removeClass("orange-color");
                $(this).parents(".nav1:eq(0)").removeClass("currentcat");
            }
            if ($(this).children(":first-child").hasClass("grayscale-off"))
            {
                $(this).children(":first-child").removeClass("grayscale-off");
            }
        });

        $(".subCategory a").each(function ()
        {
            if ($(this).hasClass("orange-color")) {
            $(this).removeClass("orange-color").css("font-weight", "normal");
            $(this).parents(".nav1:eq(0)").removeClass("currentcat");
            }
        });

        $this.parents("ul").siblings("a").addClass("orange-color").children(":first-child").addClass('grayscale-off');


        $this.children("a").addClass("orange-color").css("font-weight", "bold");
        $this.parents(".nav1:eq(0)").addClass("currentcat");
        $(".main_heading_h1").text($this.attr("data-name"));
    }
    private static _genericPreparationHandlerForCategory(requestParams)
    {
        var $this: JQuery = $("li.nav1").filter((index, el: Element) =>
        {
            return $(el).attr("data-id") == requestParams.categoryID;
        }).find("a.sf-with-ul");
        if ($this.length)
        {
            FiltrationHandlersFactory._findAndMarkForCategory($this);
        }
        else
        {
            $this = $(".subCategory").filter((index, el: Element) =>
            {
                return $(el).attr("data-id") == requestParams.categoryID;
            });

            FiltrationHandlersFactory._findAndMarkForSubcategory($this);
        }
    }

    static _findAndMarkForCategory($this: JQuery)
    {
        $(".nav1 > a").each(function (el)
        {
            if ($(this).hasClass("orange-color"))
            {
                $(this).removeClass("orange-color");
                $(this).parents(".nav1:eq(0)").removeClass("currentcat");
            }
            if ($(this).children(":first-child").hasClass("grayscale-off"))
            {
                $(this).children(":first-child").removeClass("grayscale-off");
            }
        });
        $this.children(":first-child").addClass('grayscale-off');
        $(".subCategory a").each(function ()
        {
            if ($(this).hasClass("orange-color"))
            {
                $(this).removeClass("orange-color").css("font-weight", "normal");
                $(this).parents(".nav1:eq(0)").removeClass("currentcat");
            }
        });
        $this.addClass("orange-color");
        $this.parents(".nav1:eq(0)").addClass("currentcat");
        $(".main_heading_h1").text($this.parent().attr("data-name"));
    }

    static init()
    {
        FiltrationHandlersFactory._config[FiltrationHandlersFactory.cases.SINGLE] = {
            prepare: (requestParams: IFiltrationRequestParams, data: any) =>
            {
                $("#searchProducts").val(requestParams.productName);
                delete data.categoryID;
            },
            handler: (response, status, xhr) =>
            {
                var
                    prodName = $("#searchProducts").val(),
                    ct = xhr.getResponseHeader("content-type") || "";

                if (ct.indexOf("html") > -1)
                {
                    $(".main_heading_h1").html("תוצאות החיפוש: " + prodName);
                    $("#productResultWrapper ul").html(response);
                }
                if (ct.indexOf("json") > -1)
                {
                    response = JSON.parse(response);
                    if (response.status == "productNotFound")
                    {
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
            prepare: (reqestParams: IFiltrationRequestParams, data: any) =>
            {

            },
            handler: (response, status, xhr) =>
            {
                var
                    prodName = $("#searchProducts").val(),
                    ct = xhr.getResponseHeader("content-type") || "";

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
                traditional: true,
                type: "GET"
            }
        };
        FiltrationHandlersFactory._config[FiltrationHandlersFactory.cases.CATEGORY] = {
            prepare: FiltrationHandlersFactory._genericPreparationHandlerForCategory,
            handler: (html) =>
            {
                if (html == "")
                {
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
            handler: (response, status, xhr) => {
                var
                    prodName = $("#searchProducts").val(),
                    ct = xhr.getResponseHeader("content-type") || "";

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
                type: "POST",
                traditional: true
            }
        };
        FiltrationHandlersFactory._config[FiltrationHandlersFactory.cases.AS_GALLERY] = {
            prepare: (reqestParams: IFiltrationRequestParams, data: any) =>
            {

            },
            handler: (html) =>
            {
                var
                    maintHeadingText = $(".main_heading_h1").text(),
                    $listGalleryElement = $("#listGallery");

                $listGalleryElement.removeClass("list_view_light").addClass("list_view_dark")
                $listGalleryElement.siblings("#tableGallery").removeClass("grid_view_dark").addClass("grid_view_light");
                $("#productResultWrapper ul").html(html);
                $(".main_heading_h1").text(maintHeadingText);
            },
            requestSettings: {
                type: "GET",
                traditional: true
            }
        };
    }
}

function refreshPageTitle()
{
    var
        $popup = $(".prdct_popup_h1"),
        titleText = $(".main_heading_h1").text();

    if ($popup.length)
    {
        titleText = $popup.text();
    }
    if (titleText) {
        $("title").text(titleText);
    }
    Shop.HideLoading($('.products_part'));
}

var app: Sammy.Application = Sammy(function ()
{
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
            error: function () { },
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
        factory._findAndMarkForSubcategory($("#favoriteProducts").closest("div.subCategory"));
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetFirstCategoryProducts",
            data: { shopID: shopID, favorite: true },
            cache: false,
            error: function () { },
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
        factory._findAndMarkForSubcategory($("#orderedProducts").closest("div.subCategory"));
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetFirstCategoryProducts",
            data: { shopID: shopID, allOrderedProducts: true },
            cache: false,
            error: function () { },
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
        factory._findAndMarkForSubcategory($("#dealsProducts").closest("div.subCategory"));
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetFirstCategoryProducts",
            data: { shopID: shopID, deals: true },
            cache: false,
            error: function () {
            }
        }).done(function (html) {
            $("#productResultWrapper ul").html(html);
        }).always(refreshPageTitle);
    });

    this.get("#!/:productSku/:productName", function (context) {

        if (window['dontTrigerHash'] && window['dontTrigerHash'] === true) {
            window['dontTrigerHash'] = false;
           // return;
        }
        //var productShopId = $("#productSKU_" + context.params.productSku).data('productshopid');        
        var productShopId = getCache().productShopID || context.params.productName;// $("#productSKU_" + context.params.productSku).data('productshopid');       
        //$("#productSKU_" + context.params.productSku).parents('.sh_protidctItemWrap:eq(0)').find('.product_image .fancybox:eq(0)').click();
       // return;
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetProductPopup",
            data: {
                productShopID: productShopId
            },
            cache: false,
            error: function () {
                //alert('Server error');
            },
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
            $(".popup_close1, #fade3").click(function () {
                $("#productPopup").html("");

                var cache = getCache();
                if (cache.backState != null)
                    cache.backState.productShopID = cache.productShopID;

                setCache(cache.backState);

                var backUrl = cache.backHash || "#";
                if (backUrl) {
                  //  if (backUrl.indexOf('category')
                    window['dontTrigerHash'] = true;

                    location.hash = backUrl;
                }
            });
        });
    });

    this.get("#!/product/:productShopID", function (context)
    {
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetProductPopup",
            data: {
                productShopID: context.params.productShopID
            },
            cache: false,
            error: function () {
                //alert('Server error');
            },
        }).done(function (html)
        {

            var addthis_url = "http://s7.addthis.com/js/300/addthis_widget.js#pubid=ra-55059d817ee4f3c2";
            if (window["addthis"])
            {
                window["addthis"] = null;
                window["_adr"] = null;
                window["_atc"] = null;
                window["_atd"] = null;
                window["_ate"] = null;
                window["_atr"] = null;
                window["_atw"] = null;
            }
            $.getScript(addthis_url, function (data, status, jqhr)
            {
                window["addthis"].init();
            });
            $("#productPopup").html(html).removeClass("dn");

            $("#light3").css("display", "block");
            $("#fade3").css("display", "block");
            animateToTop();
            refreshPageTitle();
            $(".popup_close1").click(function ()
            {
                $("#productPopup").html("");

                var cache = getCache();
                setCache(cache.backState);

                var backUrl = cache.backHash || "#";
                if (backUrl)
                {
                    location.hash = backUrl;
                }
            });
        });
    });

    this.get("#!/category/:shopID/:categoryID", function (context)
    {

        if (window['dontTrigerHash'] && window['dontTrigerHash'] === true) {
            window['dontTrigerHash'] = false;
            return;
        }

        var
            cacheInstance = getCache(),
            params: IFiltrationRequestParams = $.extend(context.params, cacheInstance),
            filters = params.filters,
            filtersArray = filters ? filters.toString().split(",") : [],
            requestingCase = params.requestCase || "category",
            productName = !$('#searchProducts').val() ? '' : params.productName,
        
            factory = FiltrationHandlersFactory,

            // mutual settings for all cases
            defaultAjaxRequestOptions = {
                dataType: "html",
                url: "/Shop/_GetProductByCategoryAndFilters",
                traditional: true,
                cache: false,
                error: function () {
                    //alert('Server error');
                }
            },

            data = {
                shopID: params.shopID,
                viewIn: params.viewIn,
                categoryID: params.categoryID,
                filters: filtersArray,
                productName: productName
            },
            requestSettings: JQueryAjaxSettings = factory.mergeSettings(requestingCase, defaultAjaxRequestOptions, data),
            responseHandler = factory.getResponseHandler(requestingCase),
            preparationHandler = factory.getPerparationHandler(requestingCase);

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


    


    this.get("#", function ()
    {
        console.log("root");
    });
});

function animateToTop()
{
    $("body,html").animate({
        scrollTop: 0
    }, 100);
}

$(() => app.run());