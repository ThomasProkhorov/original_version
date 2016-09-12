/*
CSS Browser Selector v0.4.0 (Nov 02, 2010)
Rafael Lima (http://rafael.adm.br)
http://rafael.adm.br/css_browser_selector
License: http://creativecommons.org/licenses/by/2.5/
Contributors: http://rafael.adm.br/css_browser_selector#contributors
*/
function css_browser_selector(u){var ua=u.toLowerCase(),is=function(t){return ua.indexOf(t)>-1},g='gecko',w='webkit',s='safari',o='opera',m='mobile',h=document.documentElement,b=[(!(/opera|webtv/i.test(ua))&&/msie\s(\d)/.test(ua))?('ie ie'+RegExp.$1):is('firefox/2')?g+' ff2':is('firefox/3.5')?g+' ff3 ff3_5':is('firefox/3.6')?g+' ff3 ff3_6':is('firefox/3')?g+' ff3':is('gecko/')?g:is('opera')?o+(/version\/(\d+)/.test(ua)?' '+o+RegExp.$1:(/opera(\s|\/)(\d+)/.test(ua)?' '+o+RegExp.$2:'')):is('konqueror')?'konqueror':is('blackberry')?m+' blackberry':is('android')?m+' android':is('chrome')?w+' chrome':is('iron')?w+' iron':is('applewebkit/')?w+' '+s+(/version\/(\d+)/.test(ua)?' '+s+RegExp.$1:''):is('mozilla/')?g:'',is('j2me')?m+' j2me':is('iphone')?m+' iphone':is('ipod')?m+' ipod':is('ipad')?m+' ipad':is('mac')?'mac':is('darwin')?'mac':is('webtv')?'webtv':is('win')?'win'+(is('windows nt 6.0')?' vista':''):is('freebsd')?'freebsd':(is('x11')||is('linux'))?'linux':'','js']; c = b.join(' '); h.className += ' '+c; return c;}; css_browser_selector(navigator.userAgent);
/*
CSS Width Selector v0.1
License: http://creativecommons.org/licenses/by/2.5/
Contributors: http://www.uco.co.il
*/
function css_width_selector() { var viewportWidth  = document.documentElement.clientWidth, viewportHeight = document.documentElement.clientHeight; var all_clases = document.documentElement.className; var all_clases = all_clases.replace(" width_big",""); var all_clases = all_clases.replace(" width_1600",""); var all_clases = all_clases.replace(" width_1200",""); var all_clases = all_clases.replace(" width_980",""); var all_clases = all_clases.replace(" width_770",""); var all_clases = all_clases.replace(" width_480",""); var all_clases = all_clases.replace(" width_320",""); if(viewportWidth <= 320) all_clases += " width_320"; else if(viewportWidth <= 480) all_clases += " width_480"; else if(viewportWidth <= 770) all_clases += " width_770"; else if(viewportWidth <= 980) all_clases += " width_980"; else if(viewportWidth <= 1200) all_clases += " width_1200"; else if(viewportWidth <= 1600) all_clases += " width_1600"; else all_clases += " width_big"; document.documentElement.className = all_clases; }
window.onresize=function(){ css_width_selector(); };
css_width_selector();