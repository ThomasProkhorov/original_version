var SearchProductData = {};
var app;
var notification;

$(document).ready(function () {
    SearchProductData.shopID = $("#shopID").val();

    notification = $("#popupNotification").kendoNotification({
        position: {
            bottom: 40,
            left: 40
        }
    }).data("kendoNotification");

    var getAutoText = function () {

        var kac = $("#searchProducts").data("kendoAutoComplete");
        var text = '';
        if (kac) {
            text = kac.value();
        }
        return {
            text: text,
            shopID: SearchProductData.shopID,
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
        placeholder: "חיפוש מוצרים בחנות"
    });

    $("input.submit_ajax_button").removeAttr("disabled");

    var checkoutFormValidator = $("#checkoutform").kendoValidator().data("kendoValidator");
    $("#checkoutFormSubmitBtn").on("click", function (event) {
        if (checkoutFormValidator.validate()) {
            document.getElementById('checkoutform').submit();
        }
        else {
        }
    });

    $("#firstCategoryProducts").on("click", function () {
        refreshProducts(null, [], true, null, true, true, true);
        initKendoMobileBtn(".productBuyBtn");
        hideDrawer();        
    });

    $("#favoriteProducts").on("click", function () {
        refreshProducts(null, [], true, null, true, false, false);
        initKendoMobileBtn(".productBuyBtn");
        hideDrawer();        
    });

    $("#orderedProducts").on("click", function () {
        refreshProducts(null, [], true, null, false, true, false);
        initKendoMobileBtn(".productBuyBtn");
        hideDrawer();        
    });

    $("#dealsProducts").on("click", function () {
        refreshProducts(null, [], true, null, false, false, true);
        initKendoMobileBtn(".productBuyBtn");
        hideDrawer();       
    });

    //select category
    $("li.nav1 > a.category").on("click", function () {
        var $this = $(this);
        var clickedPos = $(this);
       
        var elemListView = $(this).parents("ul.km-listview").data("kendoMobileListView");
        var listScroller = elemListView.scroller();

        
        $("#workCategoryText").text($this.text());
        if ($(this).siblings("ul").length > 0) {

            var selfClick = $(this).siblings("ul").is(':visible');
            if (!selfClick) {
                //$("a.categoryDetails").siblings("ul:visible").slideToggle();
            }
            $(this).siblings("ul").stop(true, true).slideToggle({
                'complete': function () {
                    return;
                  //  console.log($(this).offset());
                   // console.log(clickedPos.offset());
                    listScroller.scrollTo(0, 0);
                    var toppos =
                       // $(this).offset().top +
                        clickedPos.offset().top;// + listScroller.scrollTop;
                    if (toppos > 0) {
                        toppos *= -1;
                    }
                    listScroller.animatedScrollTo(0, toppos, 0);

                    //  console.log($(this).parents('.km-scroll-container:eq(0)').css('transform', 'translate3d(0px, -' + clickedPos.top + 'px, 0px) scale(1)'));

                }

            });

           // return;
        }
        $(".nav1 > a").each(function (el) {
            if ($(this).hasClass("orange-color")) {
                $(this).removeClass("orange-color");
            }
        });
        $(".subCategory a").each(function () {
            if ($(this).hasClass("orange-color")) {
                $(this).removeClass("orange-color");
            }
        });
        $this.addClass("orange-color");
        if (!$this.hasClass("first")) {
            SearchProductData.categoryID = $this.parent().attr("data-id");
            refreshProducts(null, [], true, SearchProductData.categoryID, false, false, false);
            initKendoMobileBtn(".productBuyBtn");
            hideDrawer();            
        }
    });

    //select subCategory
    $(".subCategory").on("click", function (e) {
        var $this = $(this);
        if ($this.hasClass("sub")) {
            $("#workCategoryText").text($this.text());
        }
        $(".nav1 > a").each(function (el) {
            if ($(this).hasClass("orange-color"))
                $(this).removeClass("orange-color");            
        });

        $(".subCategory a").each(function () {
            if ($(this).hasClass("orange-color"))
                $(this).removeClass("orange-color");
        });

        $this.parents("ul").siblings("a").addClass("orange-color");
        $this.children("a").addClass("orange-color");
        if (!$this.hasClass("first")) {
            SearchProductData.categoryID = $this.attr("data-id");
            refreshProducts(null, [], true, SearchProductData.categoryID, false, false, false);
            initKendoMobileBtn(".productBuyBtn");
            hideDrawer();            
        }
    });
    
    $("a.categoryDetails").on("click", function () {
        var clickedPos = $(this);
        var elemListView = $(this).parents("ul.km-listview").data("kendoMobileListView");
        var listScroller = elemListView.scroller();
        var selfClick = $(this).siblings("ul").is(':visible');
        if (!selfClick) {
            //$("a.categoryDetails").siblings("ul:visible").slideToggle();
        }
        $(this).siblings("ul").stop(true, true).slideToggle({
            'complete': function () {
                return;
               // console.log($(this).offset());
               // console.log(clickedPos.offset());
                listScroller.scrollTo(0, 0);
                var toppos =
                   // $(this).offset().top +
                    clickedPos.offset().top;// + listScroller.scrollTop;
                if(toppos > 0)
                {
                    toppos*=-1;
                }
                listScroller.animatedScrollTo(0, toppos, 0);
              
                //  console.log($(this).parents('.km-scroll-container:eq(0)').css('transform', 'translate3d(0px, -' + clickedPos.top + 'px, 0px) scale(1)'));

            }

        });
       
    });

    $(document.body).on("click", ".productPopupLink", function () {
        var $this = $(this);
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetProductPopup",
            data: { productShopID: $this.attr("data-productshopid") },
            cache: false,
            error: serverError,
        }).done(function (html) {            
            $("#productPopupWrapper").html(html);
            initKendoMobileBtn(".productBackBtn");
        });
    });

    $(document.body).on("click", ".showNoteWindow", function () {
        var id = $(this).attr("data-productid");
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
        var dialog = $("#note-modal-view").data("kendoMobileModalView");
        dialog.open();
    });

    $(document.body).on("click", "#addProductNote", function () {
        $this = $(this);
        var id = $this.attr("data-productid");
        var text = $("#productNoteText").val();
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
            });
    });

    app = new kendo.mobile.Application(document.body, {
        skin: 'ios',
        //transition: 'slide',
        init: function () {
            if ($("input[name='staticViewsInit']").length > 0) {
                var tabstrip = $(".tabstrip").data("kendoMobileTabStrip");
                if ($("input[name='passwordViewsInit']").length > 0) {
                   
                    if (tabstrip) {
                        $("#filterBaseBtn").hide();
                        $("#filterBtn").removeClass("dnImportant");

                        $("#homeBaseBtn").hide();
                        $("#homeBtn").removeClass("dnImportant");

                        $("#switchCart").hide();
                        $("#cartBtn").removeClass("dnImportant");

                        tabstrip.switchTo("#my-drawer");
                    }
                }
                else
                {
                    if (tabstrip) {
                        $("#filterBaseBtn").hide();
                        $("#filterBtn").removeClass("dnImportant");

                        $("#homeBaseBtn").hide();
                        $("#homeBtn").removeClass("dnImportant");

                        tabstrip.switchTo("#tabstrip-shoppingCart");
                    }
                }
            } else
                if ($("input[name='passwordViewsInit']").length > 0) {
                    var tabstrip = $(".tabstrip").data("kendoMobileTabStrip");
                    if (tabstrip) {
                        $("#filterBaseBtn").hide();
                        $("#filterBtn").removeClass("dnImportant");

                        $("#homeBaseBtn").hide();
                        $("#homeBtn").removeClass("dnImportant");

                        $("#drawerBaseBtn").hide();
                        $("#drawerBtn").removeClass("dnImportant");

                        $("#switchCart").hide();
                        $("#cartBtn").removeClass("dnImportant");

                        tabstrip.switchTo("#my-drawer");
                    }
                }
        }
    });
});

var serverError = function () { alert('Server error'); }

function hideDrawer() {
    $("#my-drawer").data("kendoMobileDrawer").hide();
}

function searchProductBtn() {
    //single search btn click
    var prodName = $("#searchProducts").val();
    refreshProducts(prodName, [], true, null, false, false, false);
    $("#workCategoryText").text("");
}

function initKendoMobileBtn(selector) {
    $(selector).kendoMobileButton();
}

function closeNoteModalView() {
    $("#note-modal-view").kendoMobileModalView("close");
}
var dataSource;
var cachedParams = false;
function mobileListViewEndlessScrollingProducts() {
    dataSource = new kendo.data.DataSource({
        //type: "aspnetmvc-ajax",
        type: "json",
        transport: {
            read: {
                traditional: true,
                data: { shopID: SearchProductData.shopID },
                type: "POST",
                dataType: 'json',
                url: "/Shop/_GetProductsForEndlessScrolling"
            },            
        },
        schema: {
            data: "Data",
            total: "Total",
            errors: "Errors",
            model: {
                fields: {
                    //filters: "filters",
                    list: "list"
                },
            }

        },
        requestEnd: function (e) {
            if (!e.response.Data || e.response.Data.length == 0) {
                if ($('#productResultWrapper .noresulttable').length == 0) {
                    $('#productResultWrapper').append($('#noresulthtml').html());
                }
            }
        },
        serverPaging: true,
        serverSorting: true,
        pageSize: 20,
        isParam: false,
    });
    if (cachedParams) {
        dataSource.transport.options.read.data = cachedParams;
        cachedParams = false;
    }
 var listViewEndless =  $("#productResultWrapper").kendoMobileListView({
        dataSource: dataSource,
        template: $("#endless-scrolling-template-products").text(),
        endlessScroll: true,
        scrollTreshold: 30,
    });

}

function refreshProducts(name, _filters, refreshFilters, categoryID, favorite, allOrderedProducts, deals, keywords) {
    app.scroller().reset();    
    var productsList = $('#productResultWrapper').data('kendoMobileListView');
    if (!productsList) {
        cachedParams = {
            shopID: SearchProductData.shopID,
            productName: name,
            categoryID: categoryID,
            favorite: favorite,
            allOrderedProducts: allOrderedProducts,
            deals: deals,
            keywords: keywords,
            page: 1,
        };
        return;
    }
    productsList.dataSource._page = 1;
    productsList.dataSource._skip = 0;   

    productsList.dataSource.transport.options.read.data = {
        shopID: SearchProductData.shopID,
        productName: name,
        categoryID: categoryID,
        favorite: favorite,
        allOrderedProducts: allOrderedProducts,
        deals: deals,
        keywords: keywords,
        page: 1,
    };

    productsList.dataSource.read();
    productsList.refresh();    
}