if (!("path" in Event.prototype))
    Object.defineProperty(Event.prototype, "path", {
        get: function () {
            var path = [];
            var currentElem = this.target;
            while (currentElem) {
                path.push(currentElem);
                currentElem = currentElem.parentElement;
            }
            if (path.indexOf(window) === -1 && path.indexOf(document) === -1)
                path.push(document);
            if (path.indexOf(window) === -1)
                path.push(window);
            return path;
        }
    });
function QueryStringToJSON(text) {
    var pairs = text.split('&');

    var result = {};
    pairs.forEach(function (pair) {
        pair = pair.split('=');
        result[pair[0]] = decodeURIComponent(pair[1] || '');
    });

    return JSON.parse(JSON.stringify(result));
}
var Shop = {
    isLogined: false,
    isMobile: false,
    emptycart: false,
    queueEvent: [],
    cartElementId: 'footercart',
    cartFullElemntId:'sh_id_shopcart',
    ChangeFreeShip: function (e,shipid,total) {
        document.getElementById('ShippingMethod').value = shipid;
        $('.shicon').removeClass('current');
        $('.shicon', $(e)).addClass('current');
        $('.sh_carttotal').html(total);
        Shop.AnimateBold($('.sh_carttotal'));

        $('#shshipttime').hide();
        $('#shworkttime').show();
        $('.deliveryManualDescription').show();
        $('.shipenable').hide();
        $('.shipdisable').show();
    },
    ChangeCostShip: function (e, shipid, total) {

        document.getElementById('ShippingMethod').value = shipid;
        $('.shicon').removeClass('current'); $('.shicon', $(e)).addClass('current');
        $('.sh_carttotal').html(total);
        Shop.AnimateBold($('.sh_carttotal'));
        $('#shworkttime').hide();
        $('#shshipttime').show();
        $('.deliveryManualDescription').hide();
        $('.shipdisable').hide();
        $('.shipenable').show();

    },
    ApplyCode: function (id, totalselector) {
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Checkout/ApplyCode/" + id,
            data: { code: $('#CouponCode').val() },
            cache: false,
            error: function () {
                //alert('Server error');
            }
        }).done(function (data) {
            if (data.result == "ok") {
                $('.couponecode-text').html(data.text);
                if (totalselector) {
                    $(totalselector).text(data.total);
                }
            } else {
                //data.message - errors
                Shop.ShowErrors(data, wrap);
            }
        });
    },
    ReOrder: function (e, event, id) {
        event.preventDefault();
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Profile/ReOrder/"+id,
            data: {},
            cache: false,
            error: function () {
                //alert('Server error');
            }
        }).done(function (data) {
            if (data.result == "ok") {
                location.href = data.url;
            } else {
                //data.message - errors
                Shop.ShowErrors(data, wrap);
            }
        });
    },
    SaveOrderDetail: function (e, event) {
        event.preventDefault();
        var wrap = $(e).parents('form:eq(0)');
        var order = wrap.serialize();
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Profile/SaveOrder",
            data: order,
            cache: false,
            error: function () {
                //alert('Server error');
            }
        }).done(function (data) {
            if (data.result == "ok") {

            } else {
                //data.message - errors
                Shop.ShowErrors(data, wrap);
            }
        });
    },
    setCookie: function (name, value, options) {
        options = options || {};

        var expires = options.expires;

        if (typeof expires == "number" && expires) {
            var d = new Date();
            d.setTime(d.getTime() + expires * 1000);
            expires = options.expires = d;
        }
        if (expires && expires.toUTCString) {
            options.expires = expires.toUTCString();
        }

        value = encodeURIComponent(value);

        var updatedCookie = name + "=" + value + "; path=/";

        for (var propName in options) {
            updatedCookie += "; " + propName;
            var propValue = options[propName];
            if (propValue !== true) {
                updatedCookie += "=" + propValue;
            }
        }

        document.cookie = updatedCookie;
    },
    UpdateCartWrap: function () {
        if ($('#' + Shop.cartElementId).length > 0) {
            $.ajax({
                type: "GET",
                dataType: "html",
                async: false,
                data: { tableonly: true },
                url: "/ShoppingCart/FooterCart/" + $('#shopID').val(),
                error: function () {
                    //alert('Server error');
                }
            }).done(function (data) {
                if (Shop.emptycart)
                {
                    Shop.emptycart = false;
                    $('.fTab').click();
                    setTimeout(function () {
                        $('.fTab').click();
                    }, 2000);
                }
                $('#' + Shop.cartElementId).html(data);
                Shop.Animate($('#' + Shop.cartElementId));
                $('.sh_carttotal').html($('#sh_carttotalmain').html());
                Shop.AnimateBold($('.sh_carttotal'));
            });
        }
        if ($('#' + Shop.cartFullElemntId).length > 0) {
            $.ajax({
                type: "GET",
                dataType: "html",
                async: false,
                data: { tableonly: true },
                url: "/ShoppingCart/_GetShoppingCartTable/" + $('#shopID').val(),
                error: function () {
                    //alert('Server error');
                }
            }).done(function (data) {
                $('#' + Shop.cartFullElemntId).html(data);
                Shop.Animate($('#' + Shop.cartFullElemntId));
                $('.sh_carttotal').html($('#sh_carttotalmain').html());
                Shop.AnimateBold($('.sh_carttotal'));
            });
        }
    },
    Animate: function (el) {
        el.fadeTo('slow', 0.5).fadeTo('slow', 1.0);
    },
    AnimateBold: function (el) {
        el.css({ 'font-weight': 700 }).fadeTo('slow', 0.7).fadeTo('slow', 1.0, function () { el.css({ 'font-weight': 'normal' }); });
    },
    AnimateAndRemove: function (el) {
        el.fadeTo('slow', 0.7).fadeTo('slow', 1.0, function () { el.remove(); });
    },
    ShowLoading: function (container) {
        if ($('.indicatorload', container).length == 0) {
            if ($('td', container).length > 9) {
                $('td:eq(9)', container).css('position','relative').append('<div class="k-loading-mask indicatorload" style="width:100%;height:100%;top:0;"><span class="k-loading-text">Loading...</span><div class="k-loading-image"><div class="k-loading-color"></div></div></div>'

               );
            } else {
                var checkpos = container.css('position');
                if (!checkpos || checkpos == 'static')
                {
                    container.css('position','relative');
                }
                container.append('<div class="k-loading-mask indicatorload" style="width:100%;height:100%;top:0;"><span class="k-loading-text">Loading...</span><div class="k-loading-image"><div class="k-loading-color"></div></div></div>'

                );
            }
        }

        $('.indicatorload', container).show();
    },
   HideLoading: function (container) {
       
        $('.indicatorload', container).hide();
    },
   AddToCart: function (proddata) {
       var _async = true;
       if (false)//!Shop.isLogined) {
       {
           _async = false;
       }
        $.ajax({
            type: "POST",
            dataType: "json",
            async: _async,
            url: "/Shop/AddToCartAjx",
            data: proddata,
            error: function () { alert('Server error, add to cart'); }
        }).done(function (data) {
           
            if (data.result == "ok") {
                Shop.UpdateCartWrap();
                qtstring = data.data.Quantity;
                if (data.data.MeasureUnit && data.data.QuantityType == 0) {
                    qtstring = qtstring.toFixed(2);
                }

               
                console.log(data);
                //measureSource_
                $('#quantityinput_' + data.data.ProductShopID).val(qtstring);
                proddata = QueryStringToJSON(proddata);

              //  $('#priceSelector_' + data.data.ProductShopID).html('₪' + (proddata.Quantity * $('#priceSource_' + proddata.ProductShopID).val()).toFixed(2)
              //      + (true ? ' / ' + $('#measureSource_' + proddata.ProductShopID).val() : ''));


                var wrap = $('#quantityinput_' + proddata.ProductShopID).parents('.sh_protidctItemWrap:eq(0)');
                var anp = {
                    'id': proddata.ProductShopID,                     // Transaction ID. Required.
                    'name': $('.product_name', wrap).text(),    // Product name. Required.
                    'sku': '',                 // SKU/code.
                    'category': $('.nav1.current a:eq(0)').text() + $('.subCategory.current a:eq(0)').text(),         // Category or variation.
                    'price': $('#priceSource_' + proddata.ProductShopID).val(),                 // Unit price.
                    'quantity': proddata.Quantity                   // Quantity.
                };
               // console.log(anp);
                ga('ecommerce:addItem', anp);
                ga('ecommerce:send');
            } else {
                if (data.action && data.action == 'login') {
                    Shop.isLogined = false;
                    Shop.queueEvent.push({
                        func: Shop.AddToCart,
                        data: data.data
                    });
                    Shop.ShowLoginReg();
                } else {
                    alert(data.message);
                }
            }
        })
       .always(function () {
           Shop.HideLoading($('.sh_protidctItemWrap '));
       });
   },
   KeepOld: function (e, event, val) {
       if (event && event.preventDefault)
       {
           event.preventDefault();
       }
       $.ajax({
           type: "POST",
           dataType: "json",
           // async: false,
           url: "/ShoppingCart/KeepOld",
           data: { keep: val },
           error: function () {
               //alert('Server error');
           }
       }).done(function (data) {
           location.reload();
       });
   },
   GetQuantityString: function (productModel) {
       var haveMeansureQuantity = false;

       if (productModel.SoldByWeight && (productModel.MeasureUnit != null || productModel.MeasureUnitStep != null)) {
           haveMeansureQuantity = true;
       }

       var qtstring = productModel.Quantity;
       if (haveMeansureQuantity) {
           qtstring = qtstring.toFixed(2);
       }
       return qtstring;
   },
    CartChange: function (data, wrap) {
        var d = data;
        $.ajax({
            type: "POST",
            dataType: "json",
           // async: false,
            url: "/ShoppingCart/ChangeItem",
            data: data,
            error: function () {
                //alert('Server error');
            }
        }).done(function (data) {
            Shop.HideLoading($('.sh_protidctItemWrap '));
            if (data.result == "ok") {
                var temp = $("#productResultWrapper .sh_protidctItemWrap input[name='ProductShopID'][value='" + data.data.ProductShopID + "']");
                if (d.Delete && temp.length > 0) {
                    if (Shop.isMobile) {
                        temp.siblings(".productPopupLink").find(".prdct_check").addClass("dn");
                    }
                    else {
                        if (SearchProductData.viewIn == "table" || SearchProductData.viewIn == "tableItemPartial") {
                            temp.parents('td').siblings(".list_prdct_clm10").find("a").removeClass("btn1_brown").addClass("btn1").text("הוסף לסל");
                        } else {
                           // temp.closest(".product_box_open").find(".product_green_btn").parent()
                          //      .toggleClass('boxbuttonhidden').toggleClass('boxbuttonshowed');//
                            //.hide();
                                //.removeClass("product_brown_btn").addClass("product_green_btn").children("a").text("הוסף לסל");
                          //  $('.prdct_qty_part', temp).toggleClass('boxbuttonhidden').toggleClass('boxbuttonshowed');//.show();
                            temp.closest(".product_box_open").siblings(".prdct_check").addClass("dn");
                        }
                        
                    }
                }
                if (wrap) {
                    var qtstring = Shop.GetQuantityString(data.data);
                    $('input[name="Quantity"]', wrap).val(qtstring);
                    if (data.data.Quantity == 0)
                    {
                        $('input[name="Quantity"]', wrap).val(1);
                        var quantityInput = $('#quantityinput_' + data.data.ProductShopID);
                        
                        //prdct_check - display: none mobile
                        if (Shop.isMobile) {
                            wrap.fadeTo(50, 0.0, function () { wrap.remove(); });
                            var mCheck = quantityInput.parents(".sh_protidctItemWrap").find('.prdct_check');
                            if (!mCheck.hasClass("dn")) {
                                mCheck.addClass("dn");
                            }
                            $('.prdct_qty_part', quantityInput.parents(".sh_protidctItemWrap")).toggleClass('boxbuttonhidden').toggleClass('boxbuttonshowed');//.hide();
                            $('.productBuyBtn', quantityInput.parents(".sh_protidctItemWrap")).parent().toggleClass('boxbuttonhidden').toggleClass('boxbuttonshowed');//.show();
                        }
                        else {
                            console.log(wrap);
                            wrap.fadeTo(300, 0, function () { wrap.remove(); });
                            //prdct_check - display: none
                            var check = quantityInput.closest('.product_box_open').siblings('.prdct_check');

                            if (!check.hasClass("dn")) {
                                check.addClass("dn");
                            }
                            //change btn color and text
                            //var btn = quantityInput.closest(".prdct_qty_part").siblings(".product_brown_btn").removeClass("product_brown_btn").addClass("product_green_btn").children("a").text("הוסף לסל");
                            $('.prdct_qty_part', quantityInput.parents('.sh_protidctItemWrap:eq(0)')).toggleClass('boxbuttonhidden').toggleClass('boxbuttonshowed');//.hide();
                            $('.product_green_btn', quantityInput.parents('.sh_protidctItemWrap:eq(0)')).parent().toggleClass('boxbuttonhidden').toggleClass('boxbuttonshowed');//.show();
                            var cartquantityInput = $('.sh_cartProductWrap[data-productshopid="' + data.data.ProductShopID + '"]');
                           // console.log(cartquantityInput, cartquantityInput.parents('.sh_cartProductWrap:eq(0)'));
                            $('.cart2_left_qty_in', cartquantityInput).addClass('dn').hide().find('[type="text"]').val('1');
                            $('.product_green_btn3', cartquantityInput).removeClass('boxbuttonhidden').addClass('boxbuttonshowed').show();
                           // if ($('.product_green_btn3', cartquantityInput).hasClass("boxbuttonhidden"))
                            //{
                            //    $('.cart2_left_qty_in', cartquantityInput).addClass('boxbuttonhidden').removeClass('boxbuttonshowed');//.hide();
                           //     $('.product_green_btn3', cartquantityInput).removeClass('boxbuttonhidden').addClass('boxbuttonshowed');//.show();

                           // }
                           
                        }
                    } else {                       
                        $('.shPrice', wrap).html(data.data.PriceStr);
                        $('.shUnitPrice', wrap).html(data.data.UnitPriceStr);

                        if (data.data.DiscountDescription) {
                            $('.cartitemdiscount', wrap).show();
                            $('.cartitemdiscount', wrap).attr('title', data.data.DiscountDescription);
                            Shop.AddToolTip($('.cartitemdiscount', wrap));
                        } else {
                            $('.cartitemdiscount', wrap).hide();
                        }

                        $('.shsku', wrap).html(data.data.Manufacturer);
                        $('.shAttributeDescription', wrap).html(data.data.AttributeDescription);
                        wrap.fadeTo('slow', 0.5).fadeTo('slow', 1.0);
                        $('#quantityinput_' + data.data.ProductShopID).val(qtstring);
                        $('#quantityinputProductPopup_' + data.data.ProductShopID).val(qtstring);
                    }
                    $('.sh_carttotal').html(data.cart.TotalStr);
                    $('.sh_cartcount').html(data.cart.Count);
                   $('.sh_cartdiscount').html(data.cart.TotalDiscountStr);
                    $('#sh_carttotalmain').html(data.cart.TotalStr);
                    $('.shopping-cart-total').html(data.cart.TotalStr);
                    $('.sh_cartdiscountdescr').html(data.cart.DiscountDescription);
                    $('.sh_carttotalwithoutship').html(data.cart.TotalWithoutShipStr);

                    if (data.cart.TotalDiscount > 0) {
                        $('#discountinfo').show();
                    } else {
                        $('#discountinfo').hide();
                    }
                    Shop.AnimateBold($('.sh_carttotal'));
                    Shop.AnimateBold($('#sh_carttotalmain'));
                }
                if (data.cart.ShippingCost > 0) {
                    $('.shipnotfree').show();
                    $('.shipfree').hide();
                } else {
                    $('.shipnotfree').hide();
                    $('.shipfree').show();
                }
                if (data.data.Quantity > 0) {
                 //   $('#priceSelector_' + data.data.ProductShopID).html('₪' + (data.data.Quantity * $('#priceSource_' + data.data.ProductShopID).val()).toFixed(2)
                 //       + (true ? ' / ' + $('#measureSource_' + data.data.ProductShopID).val() : ''));
                }
            } else {
                if (data.action && data.action == 'login') {
                    Shop.isLogined = false;
                    Shop.queueEvent.push({
                        func: Shop.CartChange,
                        data: data.data
                    });
                    Shop.ShowLoginReg();
                } else {
                    alert(data.message);
                }
            }
        });
    },
    AddToolTip: function (e) {
        e.poshytip({
            className: 'tip-green',
            offsetX: -7,
            offsetY: 16,
            alignX: 'inner-left',
            allowTipHover: true
        });
    },
    ClearCart: function (e, event, shopID, confirmmessage) {
        event.preventDefault();
        if (confirm(confirmmessage)) {
            
            var itemdata = { ID: shopID };
            var wrap = $(e).parents('footer:eq(0)');
            Shop.ShowLoading(wrap);
            $.ajax({
                type: "POST",
                dataType: "json",
                
                url: "/ShoppingCart/ClearCart",
                data: itemdata,
                error: function () { alert('Server error, clear cart'); }
            }).done(function (data) {

                if (data.result == "ok") {
                    Shop.UpdateCartWrap();
                    location.reload();
                } else {
                    
                        alert(data.message);
                    
                }
            })
    .always(function () {
        Shop.HideLoading(wrap);
    });
            
        }
    },
    CartDelete: function (e, event, id) {
       // if (confirm("Are you sure?")) {
            event.preventDefault();
            var wrap = $(e).parents('.sh_cartitem:eq(0)');
            var itemdata = { ID: id, Delete: true };
            if (false) {//!Shop.isLogined) {
                Shop.queueEvent.push({
                    func: Shop.CartChange,
                    data: itemdata
                });
                Shop.ShowLoginReg();
            } else {
                Shop.CartChange(itemdata, wrap);
            }
       // }
    },
    CartChangeAttribute: function (e, event, id) {
        event.preventDefault();
        var wrap = $(e).parents('.sh_cartitem:eq(0)');
        var itemdata = { ID: id, Attribute: $(e).val() };
        if (false){//!Shop.isLogined) {
            Shop.queueEvent.push({
                func: Shop.CartChange,
                data: itemdata
            });
            Shop.ShowLoginReg();
        } else {
            Shop.CartChange(itemdata, wrap);
        }
    },
    BoxChangeQuantityType: function (e, event, id, type,price,priceunit,dimension,step) {
        //event.preventDefault();
        step = 1;//always 1
        var wrap = $(e).parents('.sh_protidctItemWrap:eq(0)');
        $('.quntitytypespan').removeClass('currentcheck');
        $(e).addClass('currentcheck');
        var divmeansure = $('.inputquantitymeansure', wrap);
        var divunit = $('.inputquantityunit', wrap);
        if (type == 1) {
            divmeansure.hide();
            var quid = $('input[name="Quantity"]', divmeansure).attr('id');
            var val = $('input[name="Quantity"]', divmeansure).val();
            if (typeof val == 'undefined') {
                return;
            }
            var floatVal = parseFloat(val).toFixed(0);
            if (floatVal == 0)
            {
                floatVal = 1;
            }
            $('input[name="Quantity"]', divmeansure).attr('name', 'Quantity_hide').attr('id', quid+'_hide');
            divunit.show();
            $('input[name="Quantity_hide"]', divunit).attr('name', 'Quantity').attr('id', quid).val(floatVal);
            var item = $(".stripmain").find("[data-productshopid='" + id + "']");
            var nextquant = floatVal;
            $('#priceSource_' + id).val(priceunit);
            $('#measureSource_' + id).val($('#peritemdim').val());
            $('#priceSelector_' + id, wrap).html('₪' + (priceunit).toFixed(2) + (true ? ' / ' + $('#peritemdim').val() : '')); //nextquant *
            if (item.length > 0) {
                var _productID = item.data("productid");
                Shop.ShowLoading($(e).parents('.sh_protidctItemWrap:eq(0)'));

                Shop.CartChange({ ID: _productID, qtype: 1 }, $('.sh_cartitem[data-productshopid=' + id + ']'));
                return false;
            }
            if (Shop.isMobile) {
                var item = $(".shpng_cart2_tab").find("[data-productshopid='" + id + "']");

                if (item.length > 0) {
                    var _productID = item.data("productid");
                    Shop.CartChange({ ID: _productID, qtype: 1 }, $("[data-productshopid='" + id + "']"));
                    return false;
                }

            }
            
        } else {
            divunit.hide();
            var quid = $('input[name="Quantity"]', divunit).attr('id');
            var val = $('input[name="Quantity"]', divunit).val();
            if (typeof val == 'undefined')
            {
                return;
            }
            $('input[name="Quantity"]', divunit).attr('name', 'Quantity_hide').attr('id', quid + '_hide');
            divmeansure.show();
            $('input[name="Quantity_hide"]', divmeansure).attr('name', 'Quantity').attr('id', quid).val(parseFloat(val).toFixed(0));
            var item = $(".stripmain").find("[data-productshopid='" + id + "']");
            $('#priceSource_' + id).val(price);
            $('#measureSource_' + id).val(dimension);
            $('#priceSelector_' + id, wrap).html('₪' + (price / step).toFixed(2) + (true ? ' / ' + '' + dimension : '')); //val *
            if (item.length > 0) {
                var _productID = item.data("productid");
                Shop.ShowLoading($(e).parents('.sh_protidctItemWrap:eq(0)'));

                Shop.CartChange({ ID: _productID, qtype: 0 }, $('.sh_cartitem[data-productshopid=' + id + ']'));
                return false;
            }
            if (Shop.isMobile) {
                var item = $(".shpng_cart2_tab").find("[data-productshopid='" + id + "']");

                if (item.length > 0) {
                    var _productID = item.data("productid");
                    Shop.CartChange({ ID: _productID, qtype: 0 }, $("[data-productshopid='" + id + "']"));
                    return false;
                }

            }
        }
    },
    BoxDecrease: function (e, event, id, changeInCart) {
        if (event) { event.preventDefault() };
        var wrap = $(e).parents('.sh_protidctItemWrap:eq(0)');
        var inp = $('input[name="Quantity"]', wrap);
        var parentArguments = arguments;

        inp.css('top', '');
        inp.animate({ 'top': '30px' }, {
            duration: 300, queue: false, always: function () {
                var vl = inp.val();
                var stp = 1;
                if (parentArguments.length > 4 && parentArguments[4]) {
                    stp = parentArguments[4];
                }
                var n = parseFloat(vl) - stp;
                if (parentArguments.length > 5 && parentArguments[5]) {
                    n = parseFloat(parentArguments[5]).toFixed(2);
                }
                if (changeInCart) {
                    if (Shop.isMobile) {
                        var item = $(".shpng_cart2_tab").find("[data-productshopid='" + id + "']");

                        if (item.length > 0) {
                            var _productID = item.data("productid");

                            Shop.CartDecrease(item.find(".shpng_qty_show"), event, _productID, stp);
                            if (!n || n <= 0) { n = 0; }
                            inp.val(n);
                            inp.css('top', '');
                            return false;
                        }
                    }
                    else {
                        var item = $(".stripmain, .shoppingCartProducts").find("[data-productshopid='" + id + "']");

                        if (item.length > 0) {
                            var _productID = item.data("productid");
                            Shop.ShowLoading($(e).parents('.sh_protidctItemWrap:eq(0)'));

                            Shop.CartDecrease(item.find(".prdct_qty_part, .shpng_qty_show"), event, _productID, stp);
                            if (!n || n <= 0) { n = 0; }
                            inp.val(n);
                            inp.css('top', '');
                            // return false;
                        }
                    }
                }
                if (!n || n <= 0) { n = 0; }
                inp.val(n);
                inp.css('top', '');
                if (parentArguments.length > 6 && parentArguments[6] && parentArguments.length > 7 && parentArguments[7]) {
                    if (n == 0) { n = 1; }
                   // $('#priceSelector_' + id, wrap).html('₪' + (n * parentArguments[6]).toFixed(2) + (true ? ' / ' + parentArguments[7] : ''));
                } else if (parentArguments.length > 6 && parentArguments[6]) {
                    if (n == 0) { n = 1; }
                  //  $('#priceSelector_' + id, wrap).html('₪' + (n * parentArguments[6]).toFixed(2) + (true ? ' / ' + $('#peritemdim').val() : ''));

                }
            }
        });
      
       
    },
    BoxIncreas: function (e, event, id, changeInCart) { //productShopID but in cart Product id
        if (event) { event.preventDefault() };
        
        var wrap = $(e).parents('.sh_protidctItemWrap:eq(0)');
        var inp = $('input[name="Quantity"]', wrap);

        var parentArguments = arguments;
        inp.css('top', '');
        inp.animate({ 'top': '-30px' }, {
            duration: 300, queue: false, always: function () {
                var stp = 1;
                if (parentArguments.length > 4 && parentArguments[4]) {
                    stp = parentArguments[4];
                }
                var vl = inp.val();


                var n = parseFloat(vl) + stp;
                if (parentArguments.length > 5 && parentArguments[5]) {
                    n = parseFloat(parentArguments[5]).toFixed(2);
                }
                if (changeInCart) {
                    if (Shop.isMobile) {
                        var item = $(".shpng_cart2_tab").find("[data-productshopid='" + id + "']");

                        if (item.length > 0) {
                            var _productID = item.data("productid");

                            Shop.CartIncreas(item.find(".shpng_qty_show"), event, _productID, stp);
                            if (!n || n <= 0) { n = 0; }
                            inp.val(n);
                            inp.css('top', '');
                            return false;
                        }
                    }
                    else {
                        var item = $(".stripmain, .shoppingCartProducts").find("[data-productshopid='" + id + "']");

                        if (item.length > 0) {
                            var _productID = item.data("productid");
                            Shop.ShowLoading($(e).parents('.sh_protidctItemWrap:eq(0)'));

                            Shop.CartIncreas(item.find(".prdct_qty_part, .shpng_qty_show"), event, _productID, stp);
                            if (!n || n <= 0) { n = 0; }
                            inp.val(n);
                            inp.css('top', '');
                            //return false;
                        }
                    }
                }
                if (!n || n <= 0) { n = 0; }
                inp.val(n);
                inp.css('top', '');
                if (parentArguments.length > 6 && parentArguments[6] && parentArguments.length > 7 && parentArguments[7]) {
                    if (n == 0) { n = 1; }
                 //   $('#priceSelector_' + id, wrap).html('₪' + (n * parentArguments[6]).toFixed(2) + (true ? ' / ' + parentArguments[7] : ''));
                } else if (parentArguments.length > 6 && parentArguments[6]) {
                    if (n == 0) { n = 1; }
                  //  $('#priceSelector_' + id, wrap).html('₪' + (n * parentArguments[6]).toFixed(2) + (true ? ' / ' + $('#peritemdim').val() : ''));
                  
                }
            }
        });


       
    },
    CartDecrease: function (e, event, id, step) {
        var value = $(e).parent().next().children('.single-line').val();
        var confirmation = true;
        if (value == 1) {
            confirmation = confirm("Are you sure?");
        }
        if (confirmation) {
            if (event) { event.preventDefault() };
            var wrap = $(e).parents('.sh_cartitem:eq(0)');
            var itemdata = { ID: id, QuantityBit: -step };
            if (false) {//!Shop.isLogined) {
                Shop.queueEvent.push({
                    func: Shop.CartChange,
                    data: itemdata
                });
                Shop.ShowLoginReg();
            } else {
                Shop.CartChange(itemdata, wrap);
            }
        }
    }, 
    CartQuantKeyPress: function (e, event, id) {
        //return;
        if (event.which == 13) {
            var wrap = $(e).parents('.sh_cartitem:eq(0)');
            var step = $('input[name="Quantity"]', wrap).val();
            var itemdata = { ID: id, Quantity: step };
            if (false) {//!Shop.isLogined) {
                Shop.queueEvent.push({
                    func: Shop.CartChange,
                    data: itemdata
                });
                Shop.ShowLoginReg();
            } else {
                Shop.CartChange(itemdata, wrap);
            }
        }
    },
    CartIncreas: function (e, event, id, step) {
        if (event) { event.preventDefault() };
        Shop.ShowLoading ($(e).parents('.sh_protidctItemWrap:eq(0)'));
        var wrap = $(e).parents('.sh_cartitem:eq(0)');
        var itemdata = { ID: id, QuantityBit: step };
        if (false) {//!Shop.isLogined) {
           Shop.queueEvent.push({
               func: Shop.CartChange,
               data: itemdata
            });
            Shop.ShowLoginReg();
        } else {
            Shop.CartChange(itemdata, wrap);
        }
       
    },
    BoxChangeAttribute: function (e, event, id) {
        var p = $(e).parents('.sh_protidctItemWrap:eq(0)');
        $('input[name="ProductAttributeOptionID"]', p).val($(e).val());
       
        $('.sh_boxprice', p).html($("option:selected", $(e)).attr('data-price'));
        $('.sh_boxpriceunit', p).val($("option:selected", $(e)).attr('data-priceunit'));
        $('input[name="Quantity"]', p).trigger('change');
        
    },
    BuyKeyCart: function (e, event, id) {
        if (event.which == 13) {
            //quantityType: 0
            //Quantity: 0.6
            //Quantity_hide: 22
            //ProductShopID: 332516
            //ShopID: 46
            //OverrideQuantity: 1.6
            var product = $('<form>').append($(e).parents('.sh_cartitem:eq(0)').clone()).serialize();
               product += '&OverrideQuantity=' + ($(e).parents('.sh_cartitem:eq(0)').find('input[name="Quantity"]').val());
               Shop.ShowLoading($(e).parents('.sh_cartitem:eq(0)'));
               Shop.AddToCart(product);
        }
    },
    BuyKey: function (e, event, id) {
        $(e).data('ischanged',true);
        if (event.which == 13) {
            Shop.CancelBuyIfAdded = false;
            Shop.Buy(e, event, id);
            $(e).data('ischanged', false);
        }
    },
    BuyOnBlur: function (e, event, id) {
        if ($(e).data('ischanged')) {
            Shop.CancelBuyIfAdded = false;
            Shop.Buy(e, event, id);
            $(e).data('ischanged', false);
        }
        
    },
    CancelBuyIfAdded:true,
    Buy: function (e, event, id) {
        event.preventDefault();

        var product = $('<form>').append($(e).parents('.sh_protidctItemWrap:eq(0)').clone()).serialize();
        if (product.indexOf('ProductAttributeOptionID=0') > -1)
        {
            var par = $(e).parents('.sh_protidctItemWrap:eq(0)');
            var sel = $('[name="ProductAttributeOptionID_box"]', par);
            if (sel.parent('div.product_box_select').length > 0) {
                
               sel.parent('.product_box_select').css('border', '1px solid red');
               sel.parent('.product_box_select').before('<div class="sh_boxwarning" style="margin-bottom:15px;">Need select option</div>');
               $('.sh_boxwarning', par).fadeTo('slow', 0.3).fadeTo('slow', 1.0, function () {
                   $(this).remove();
                   sel.parent('.product_box_select').css('border', 'none');
               });
            } else
                if (sel.parent('div.pop_prdct_select1').length > 0) {
                    sel.parent('.pop_prdct_select1').css('border', '1px solid red');
                    sel.parent('.pop_prdct_select1').before('<div class="sh_boxwarning" style="margin:15px 0;">Need select option</div>');
                    $('.sh_boxwarning', par).fadeTo('slow', 0.3).fadeTo('slow', 1.0, function () {
                        $(this).remove();
                        sel.parent('.pop_prdct_select1').css('border', '1px solid #c7c4b8');
                    });
                }
                else {
                    if (Shop.isMobile) {
                        par.find(".productPopupLink")[0].click();
                    }
                    else {
                        sel.wrap('<div class="sh_boxwarning">Need select option</div>');
                        $('.sh_boxwarning', par).children('[name="ProductAttributeOptionID_box"]').css('border', '1px solid red');
                        $('.sh_boxwarning', par).fadeTo('slow', 0.3).fadeTo('slow', 1.0, function () {
                            var child = $(this).children();
                            $(this).html(child)
                            $('.sh_boxwarning', par).children('[name="ProductAttributeOptionID_box"]').css('border', '1px solid #c7c4b8');
                            $(this).children('[name="ProductAttributeOptionID_box"]').unwrap();
                        });
                    }
                }          
          
           return;
        }
        if (product.indexOf('Quantity=0&') > -1) {
            product = product.replace('Quantity=0&', 'Quantity=1&');
        }
        //sh_protidctItemWrap container
        if (false) {//!Shop.isLogined) {
            //set async data for future adding to cart
            Shop.queueEvent.push({
                func: Shop.AddToCart,
                data: product
            });
            Shop.ShowLoading($(e).parents('.sh_protidctItemWrap:eq(0)'));
            Shop.ShowLoginReg();
            //save shoping cart item in temp (session) and add after success login
        } else {
            var sel;
            if (Shop.isMobile) {
                sel = $(e).parents('.sh_protidctItemWrap:eq(0)').find(".prdct_check");
                notification.show($("#productSuccessfullyAdded").val(), "success");
                if (sel.length > 0) {
                    sel.removeClass("dn");                    
                }
                // $(e).find('span').text($("#itemInCartText2").val());
                $(e).hide();
                $('.prdct_qty_part', $(e).parents('.sh_protidctItemWrap:eq(0)')).show();
            }
            else {
                if (SearchProductData.viewIn == "table" || SearchProductData.viewIn == "tableItemPartial") {
                    sel = $(e).parents('.sh_protidctItemWrap:eq(0)').children("td.list_prdct_clm10").find("a").removeClass("btn1").addClass("btn1_brown").text($("#itemInCartText").val());
                }
                else {
                    //gallery
                    sel = $(e).parents('.sh_protidctItemWrap:eq(0)').children(".prdct_check");
                    if (sel.length > 0) {
                        sel.removeClass("dn");
                        var temp = sel.siblings(".product_box_open").find(".product_green_btn");

                        //if (temp.hasClass("littleBtn")) {
                        //    temp.removeClass("product_green_btn").addClass("product_brown_btn").find("a").text($("#itemInCartText2").val());
                        //}
                        //else {
                        //    temp.removeClass("product_green_btn").addClass("product_brown_btn").find("a").text($("#itemInCartText").val());
                        //}
                        $('.prdct_qty_part', $(e).parents('.sh_protidctItemWrap:eq(0)')).removeClass('boxbuttonhidden').addClass('boxbuttonshowed');//.show();
                        $('.product_green_btn', $(e).parents('.sh_protidctItemWrap:eq(0)')).parent().addClass('boxbuttonhidden').removeClass('boxbuttonshowed');//.hide();
                        $('.cart2_left_qty_in', $(e).parents('.sh_protidctItemWrap:eq(0)')).removeClass('boxbuttonhidden').addClass('boxbuttonshowed');//.show();
                        $('.product_green_btn3', $(e).parents('.sh_protidctItemWrap:eq(0)')).parent().addClass('boxbuttonhidden').removeClass('boxbuttonshowed');//.hide();

                        //cart2_left_qty_in
                        //product_green_btn3
                    }
                    else
                        if ($(e).hasClass("product_green_btn2")) {
                            var $this = $(e);
                            $this.removeClass("product_green_btn2").addClass("product_brown_btn2").children("a").text($("#itemInCartText").val());
                            $(".product_open_row2 input[name='ProductShopID'][value='" + id + "']").parents(".product_box_open").siblings(".prdct_check").removeClass("dn").end().end()
                                .siblings(".product_green_btn").removeClass("product_green_btn").addClass("product_brown_btn").children("a").text($("#itemInCartText").val());;
                            $('.prdct_qty_part', temp).show();
                        } else {
                            $('.cart2_left_qty_in', $(e).parents('.sh_protidctItemWrap:eq(0)')).show();
                            $('.product_green_btn3', $(e).parents('.sh_protidctItemWrap:eq(0)')).hide();

                        }

                }
               
            }
            var item = $(".stripmain").find("[data-productshopid='" + id + "']");

            if (item.length > 0) {
                if (Shop.CancelBuyIfAdded)
                {
                    Shop.CancelBuyIfAdded = true;
                   // return;
                }
             //   product += '&OverrideQuantity=' + ($(e).parents('.sh_protidctItemWrap:eq(0)').find('input[name="Quantity"]').val()-0+1);
                
            } else
            {
                if(Shop.isMobile)
                {
                    product += '&isMobile=true';
                }
            }
            if (!Shop.CancelBuyIfAdded)
            {
                Shop.CancelBuyIfAdded = true;
            }
            Shop.CancelBuyIfAdded = true;
            //simple add to cart
           // console.log(product);
            Shop.ShowLoading($(e).parents('.sh_protidctItemWrap:eq(0)'));
            Shop.AddToCart(product);
        }
    },
    SendSMS: function (e, event) {
        event.preventDefault();
        var smsdata = $(e).parents('form:eq(0)').serialize();
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Checkout/SendSms",
            data: smsdata,
            cache: false,
            error: function () {
                //alert('Server error');
            }
        }).done(function (data) {
            if (data.result == "ok") {
               
            } else {
                //data.message - errors
                Shop.ShowErrors(data, $(e).parents('form:eq(0)'));
            }
        });
    },
    CheckSMSCode: function (e, event) {
        event.preventDefault();
        var smsdata = $(e).parents('form:eq(0)').serialize();
        smsdata += '&OrderID=' + Shop.OrderID;
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Checkout/CheckSmsCode",
            data: smsdata,
            cache: false,
            error: function () {
                //alert('Server error');
            }
        }).done(function (data) {
            if (data.result == "ok") {
                document.getElementById('light2').style.display = 'none';
                document.getElementById('fade').style.display = 'none';
            } else {
                //data.message - errors
                Shop.ShowErrors(data, $(e).parents('form:eq(0)'));
            }
        });
    },
    ShowSMSWindow: function () {
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetSMSPopup",
            cache: false,
        }).done(function (html) {
            if (Shop.isMobile)
            {
                $("#SMSPopup").html(html).removeClass("dn");
                //$("#light2").css("display", "block");
                //$("#fade").css("display", "block");
            }
            else
            {
                $("#SMSPopup").html(html).removeClass("dn");
                $("#light2").css("display", "block");
                $("#fade").css("display", "block");
            }
        });
    }
    ,ShowRegModal: function()
    {
        $('.modalReg').modal('show');
        setTimeout(function () {
            $('#socialfacebookreg').append($('#facebookbutton'));
        }, 300);
        setTimeout(function () {
            $('[name="LastName"]')[0].focus();
        }, 300);
        
    }
    ,
    ShowLoginReg: function () {
        
        $('.myModal.login-window').modal('show');
        setTimeout(function () {
            $('#socialfacebook').append($('#facebookbutton'));
        }, 300);
        setTimeout(function () {
            $('[name="UserName"]')[0].focus();
        }, 300);
       // $('#socialfacebook').attr('id', 'socialfacebookadded');
        //$('#fb-login-button-idnew span').width("500px");
        //$('#fb-login-button-idnew span').height("70px");
        //$('#fb-login-button-idnew iframe').width("500px");
        //$('#fb-login-button-idnew iframe').height("70px");
        return;
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetLoginSignupPopup",
            cache: false,
        }).done(function (html) {
            var light3 = document.getElementById('light3');
            if (light3) {
                light3.style.display = 'none'
            }
            var fade3 = document.getElementById('fade3');
            if (fade3) {
                fade3.style.display = 'none';
            }
            if (Shop.isMobile) {
                location.hash = "#tabstrip-login";
            }
            else {
                if ($("#changeShopPopup").length == 0)
                {
                    $('body').append('<div class="dn" id="changeShopPopup"></div>');
                }
                $("#changeShopPopup").html(html).removeClass("dn");
                animateToTop();
                $("#light6").css("display", "block");
                $("#fade").css("display", "block");
                $("body").css("overflow-x", "hidden");

                FB.XFBML.parse();
            }
        }).always(function () {
            Shop.HideLoading(".sh_protidctItemWrap")
        });
    },
    GoTo: function (url) {
        location.href = url;
    },
    redirectUrl:false,
    ShowForgotPass: function (e, event) {
        event.preventDefault();
        $.ajax({
            type: "Get",
            dataType: "html",
            url: "/Shop/_GetForgotPopup",
           // data: { shopID: 1 },
            cache: false,
        }).done(function (html) {
            if (!Shop.isMobile) {
                $("#changeShopPopup").html(html).removeClass("dn");
                $("#light6").css("display", "block");
                $("#fade").css("display", "block");
            } else {
                $("#forgotwrap").html(html);
                
            }
        });
    },
    DoSendPass: function (e, event) {
        event.preventDefault();
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Account/SendPasswordAjx",
            data: $(e).parents('form:eq(0)').serialize(),
            cache: false,
            error: function () {
                //alert('Server error');
            }
        }).done(function (data) {
            if (data.result == "ok") {
                while (p = Shop.queueEvent.pop()) {
                    p.func(p.data);
                }
                location.reload();
            } else {
                //data.message - errors
                Shop.ShowErrors(data, $(e).parents('form:eq(0)'));
            }
        });
    },
    DoGoogle:function(data)
    {
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Account/GoogleAjx",
            data: data,
            cache: false,
            error: function () {
                //alert('Server error');
            }
        }).done(function (data) {

            if (data.result == "ok") {
                while (p = Shop.queueEvent.pop()) {
                    p.func(p.data);
                }
                if (Shop.redirectUrl) {
                    location.href = Shop.redirectUrl;
                } else {
                    location.reload();
                }
            } else {
                //data.message - errors
                Shop.ShowErrors(data, $(e).parents('form:eq(0)'));
            }
        });
    },
    DoFacebook: function (token, returnURL) {
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Account/FacebookAjx",
            data: { token: token },
            cache: false,
            error: function () {
                //alert('Server error');
            }
        }).done(function (data) {
           
            if (data.result == "ok") {
                while (p = Shop.queueEvent.pop()) {
                    p.func(p.data);
                }
                if (returnURL) {
                    $(location).attr('href', returnURL);
                    return false;
                }
                if (Shop.redirectUrl) {
                    location.href = Shop.redirectUrl;
                } else {
                    location.reload();
                }
            } else {
                //data.message - errors
                Shop.ShowErrors(data, $(e).parents('form:eq(0)'));
            }
        });
    },
    DoReg: function (e, event) {
        event.preventDefault();
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Account/RegAjx",
            data: $(e).parents('form:eq(0)').serialize(),
            cache: false,
            error: function () {
                //alert('Server error(registration)');
            }
        }).done(function (data) {
            if (data.result == "ok") {
                while (p = Shop.queueEvent.pop()) {
                    p.func(p.data);
                }
                if (Shop.redirectUrl) {
                    location.href = Shop.redirectUrl;
                } else {
                    location.reload();
                }
            } else {
                //data.message - errors
                Shop.ShowErrors(data, $(e).parents('form:eq(0)'));
            }
        });
    },
    RedirectOnly:false,
    DoLogin: function (e, event) {
        event.preventDefault();
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Account/LogOnAjx",
            data: $(e).parents('form:eq(0)').serialize(),
            cache: false,
            error: function () { alert('Server error or youe is too much time enter wrong password, Try again later'); }
        }).done(function (data) {
            if (data.result == "ok") {
                while (p = Shop.queueEvent.pop()) {
                    p.func(p.data);
                }
                
                if (Shop.isMobile) {
                    window.location = '/checkout/index/';
                }
                else if (!Shop.RedirectOnly && data.haveOld)
                {
                    location.reload();
                }
                else {
                    if (Shop.redirectUrl) {
                        location.href = Shop.redirectUrl;
                        return;
                    } else {
                        location.reload();
                    }
                }
            } else {
                Shop.ShowErrors(data, $(e).parents('form:eq(0)'));
            }
        });
    },

    ShowErrors: function (data,container) {
        //data.message - errors
        var errors = '';
        var errorsHTML = '';
        for (var m in data.message) {
            for (var s in data.message[m].errors) {
                errors += data.message[m].errors[s] + "\n";
                errorsHTML += '<div class="k-widget k-tooltip-validation" ><span class="k-icon k-warning"> </span>' + data.message[m].errors[s] + "</div>";
            }
        }
        if (container) {
            if (!$('.errror-wrap', container).length)
            {
                container.prepend('<div class="errror-wrap"></div>');
            }
            $('.errror-wrap', container).html(errorsHTML);
        } else {
            alert(errors);
        }
    },
    //Product searching
    //requset data
    SearchData:{},
    SearchProduct: function () {
        if (Shop.isMobile && Shop.SearchData["keywords"]) {
            refreshProducts(null, [], false, null, false, false, false, Shop.SearchData["keywords"]);
            $("#workCategoryText").text("");
            window.location.hash = '#drawer-index';
        }
        else
        {
            if ('productName' in Shop.SearchData && !$('#searchProducts').val())
            {
                console.log($('#searchProducts').val());
                Shop.SearchData['productName'] = '';
                delete Shop.SearchData['productName'];
            }
            $.ajax({
                type: "Get",
                url: "/Shop/_GetProductByCategoryAndFilters",
                data: Shop.SearchData,
                cache: false,
            }).done(function (response, status, xhr) {
                var ct = xhr.getResponseHeader("content-type") || "";
                if (ct.indexOf("html") > -1) {
                    $(".main_heading_h1").html("תוצאות החיפוש: " + Shop.SearchData["keywords"]);
                    $("#productResultWrapper ul").html(response);
                    animateToTop();
                }
                if (ct.indexOf("json") > -1) {
                    if (response.status == "productNotFound") {
                        $("#productResultWrapper ul").html("<li class='paymnt_style_in'>" + response.localizationTextComponent + "</li>");
                        $(".main_heading_h1").html(response.localizationMessage);
                    }
                }

            }).always(function () {
                Shop.HideLoading($('#productResultWrapper .paymnt_style_in, #productResultWrapper .products_part:eq(0)'));

            });
        }
    },
    PollVote: function (e, event, form) {
        event.preventDefault();
        data = $('#' + form).serialize();
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/Poll/Vote",
            data: data,
            cache: false,
        }).done(function (data) {
            $('#pollresult').html(data.message);
        });
    },
    TextSearchKeyPress: function (e, event, shopID) {
        return;//deprecated, don`t change
    if (event.which == 13) {

        Shop.SearchData["shopID"] = ShopID;
        var text = $.trim($('#shkeywords').val());
        if (!text) { return; }
        Shop.SearchData["keywords"] = text;
        Shop.SearchProduct();
        $('#shclearkeywords').show();
    }
}, TextSearchClear: function (e, event, shopID) {
    event.preventDefault();
    Shop.SearchData["shopID"] = ShopID;
    $('#shkeywords').val('')
    Shop.SearchData["keywords"] = '';
    $('#shclearkeywords').hide();
    $('#shoping_list_box').modal('hide');
    Shop.ShowLoading($('#productResultWrapper .paymnt_style_in, #productResultWrapper .products_part:eq(0)'));
    Shop.SearchData['categoryID'] = 0;
    Shop.SearchProduct();

},
    TextSearch: function (e, event, shopID) {
        event.preventDefault();
        if (Shop.isMobile) {
            Shop.SearchData["shopID"] = shopID;
        }
        else {
            Shop.SearchData["shopID"] = ShopID;
        }
        var text = $.trim($('#shkeywords').val());
        if (!text || arguments.length > 3 ) {
            text = $.trim($('#searchProducts').val());
        }
        if (!text) {
            return;
          
        }
        Shop.SearchData["keywords"] = text;
        Shop.ShowLoading($('#productResultWrapper .paymnt_style_in, #productResultWrapper .products_part:eq(0)'));
        Shop.SearchProduct();
        $('#shoping_list_box').modal('hide');
        $('#shclearkeywords').show();
        ga('send', 'event', 'Shop', 'search', text);
    },
    ImageClickDetect: function(element,e)
    {
        console.log(e);
        if (e.path.indexOf(element) < 2)
        {
            if ($(element).parents('.sh_protidctItemWrap:eq(0)').find('.product_image a:eq(0)').attr('href')) {
               

                //try change hash
                var link = $(element).parents('.sh_protidctItemWrap:eq(0)').find('.productPopupLink:eq(0)');
                // #!/8000150655190/פרסקאטי 500 מ"ל סופריורה קנדידה
                // window.dontTrigerHash = true;
                var cache = getCache();
                cache.backHash = location.hash;
                var cacheInstance = new RouterSharedCache();

                cacheInstance.backState = getCache();
                cacheInstance.backHash = location.hash;
                cacheInstance.productShopID = link.attr('data-productshopid');


                setCache(cacheInstance);
                location.hash = '#!/' + link.attr('data-productsku') + '/' + link.attr('data-productshopid');

            }
          // console.log(e, element, e.path.indexOf(element), $(element).parents('.sh_protidctItemWrap:eq(0)'), $(element).parents('.sh_protidctItemWrap:eq(0)').find('.product_image .fancybox:eq(0)'));

        }
      }
};