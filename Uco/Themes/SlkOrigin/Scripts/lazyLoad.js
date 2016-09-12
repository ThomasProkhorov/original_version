var busy = false;
var lazyLoadStop = false;
var SearchProductData = {};

$(document).ready(function () {
    $(window).scroll(function () {
        if ($(document).height() - $(window).height() <= $(window).scrollTop() + 450) {

            if (busy == false && lazyLoadStop == false) {
                var lastProductNum = $(".lastProductNum").last().attr("data-last-item-num");
                var type = $(".lastProductNum").last().attr("data-type");                
                if (lastProductNum != null && lastProductNum != undefined) {
                    GetProducts(type, lastProductNum);
                }
            }
        };
    });
});

function GetProducts(type, skip) {
    if (SearchProductData.viewIn == "table") SearchProductData.viewIn = "tableItemPartial";
    
    // some *.gif 
    Shop.ShowLoading($('.products_part:eq(0)'));
    Shop.ShowLoading($('.products_part:last'));
    $.ajax({
        type: "Post",
        url: "/Shop/_GetProductByCategoryAndFilters",
        data: { shopID: SearchProductData.shopID, skip: skip, viewIn: SearchProductData.viewIn, categoryID: SearchProductData.categoryID, filters: SearchProductData.filters },
        //data: SearchProductData,
        dataType: "html",
        cache: false,
        traditional: true,  
        beforeSend: function () {
            busy = true;            
        },
        error: function() {
            lazyLoadStop = true;
        },
        success: function (result, e) {
            if ($.trim(result) == "") {
                lazyLoadStop = true;
                return false;
            }
            if (type = "group") {
                $(".products_tab").last().after(result);                
            }
            if (type = "tableItemPartial") {
                $(".listing_tab").last().after(result);               
            }
        },
        complete: function (result) {
            // end *.gif
            busy = false;
             Shop.HideLoading($('.products_part'));
           
        }
    });   
};