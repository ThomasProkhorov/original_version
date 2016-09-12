(function ($) {

    $.fn.ivtooltip = function (options) {
        var settings = $.extend({
            // These are the defaults.
            text: '',
            
        }, options);
        this.addClass('shtooltipparent').append('<span class="shtooltip">' + settings .text+ '</span>');
       // console.log(settings);
        return this;
 };
}(jQuery));


(function ($) {

    $.fn.ivupdown = function (options) {

        // This is the easiest way to have default options.
        var settings = $.extend({
            // These are the defaults.
            dimension: '',
            min: 0.05,
            max: false,
            step: 1,
            price: 11.6,
            color: "#bebebe",
            currency: '₪',
            priceSelector: '',
            priceSource: '',
            onIncrease: false,
            onDecrease: false,
            // backgroundColor: "white"
        }, options);
        if (settings.max < settings.min) {
            settings.max = 0;
        }
        // wrapping
        this.addClass('ivupdowninput').wrap(function () {
            return "<div class='ivupdownwrap display-row'></div>";
        }).before('<div class="dcell cminus"><div class="prdct_qty_minus"><a href="#" class="ivminus">-</a></div></div>')
            
        .after('<div class="dcell cplus"><div class="prdct_qty_plus "><a href="#" class="ivplus">+</a></div></div>')
             .after('<div class="dcell ivcellcontent"><i class="ivupdowndim">' + settings.dimension + '</i></div>')
        //.before('<span class="ivupdowndimprice">(' + settings.currency + settings.price.toFixed(2) + ')</span>')
       
      //  .before('<div class="ivtopborder"></div>')
        // .before('<div class="ivbotborder"></div>');
        .wrap(function () {
            return '<div class="dcell ivcellcontent"></div>';
        });
        if (!this.val() || this.val() < settings.min) {
            this.val(settings.min);
        }

        this.parents('.ivupdownwrap').find('.ivplus').click(onPlus);
        this.parents('.ivupdownwrap').find('.ivminus').click(onMinus);

        this.bind('input', function (e) {
            $(this).val(checkMax($(this).val()));
        });
        this.bind('change', function (e) {
            
            $(this).val(validateInput($(this).val()));
        });
        //actions
        function checkMax(val) {
            var sufix = '';
            if (val[val.length - 1] == '.')
            {
                sufix = '.';
            }
            val = parseFloat(val);
            if (!val) { val = 0; }
            if (settings.max && val > settings.max) {
                val = settings.max;
            }
            return val + sufix;
        }
        function validateInput(val) {
            val = checkMax(val);

            if (!val || val < 0)//settings.min) {
            {
                val = 0;//settings.min;
            }
            
            var mod = ((val)*100) % (settings.step*100);
            if (Math.round(mod) != 0) {
                val = (Math.round(val * 100) / 100).toFixed(2);// (Math.round(val * 100 - Math.round(mod)) / 100).toFixed(2);
            } else
            {
                val = (Math.round(val * 100 ) / 100).toFixed(2);
            }
         
            
            //show price
            if (settings.priceSelector)
            {
                if (settings.priceSource) {
                    var newprice = parseFloat($(settings.priceSource).val());
                    if (!newprice)
                    {
                        newprice = 1;
                    }
                    settings.price = newprice;
                }
                var mensures = '';
                if (options.step)
                {
                    mensures = ' / ' +  options.dimension;
                }
                var showVal = val;
                if (showVal < 0.0001)
                {
                    showVal = 1;
                }
                //$(settings.priceSelector).html(settings.currency + (showVal * settings.price / settings.step).toFixed(2) + mensures);
            }
            
            return val;//.toFixed(2);
        }
        function onPlus(event) {
            event.preventDefault();
            var input = $(this).parents('.ivupdownwrap:eq(0)').find('.ivupdowninput');
            var val = input.val();
            var v = validateInput(val);// - 0 + settings.step);
          //  input.val(v);
            if (options.onIncrease) {
                options.onIncrease(v);
            }
        }
        function onMinus(event) {
            event.preventDefault();
            var input = $(this).parents('.ivupdownwrap:eq(0)').find('.ivupdowninput');
            var val = input.val();
            var v = validateInput(val);// - settings.step);
           
            input.val(v);
            if (options.onDecrease) {
                options.onDecrease(v);
            }
        }
        return this;

    };

}(jQuery));
