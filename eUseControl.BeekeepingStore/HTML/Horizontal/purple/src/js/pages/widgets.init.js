/*
Template Name: Minton - Responsive Bootstrap 4 Admin Dashboard
Author: CoderThemes
Website: https://coderthemes.com/
Contact: support@coderthemes.com
File: Widgets init js
*/
$( document ).ready(function() {
    
    var DrawSparkline = function() {
        $('#sparkline1').sparkline([0, 23, 43, 35, 44, 45, 56, 37, 40], {
            type: 'line',
            width: "100%",
            height: 165,
            chartRangeMax: 50,
            lineColor: '#7266ba',
            fillColor: 'rgba(114,102,186,0.3)',
            highlightLineColor: 'rgba(0,0,0,.1)',
            highlightSpotColor: 'rgba(0,0,0,.2)',
            maxSpotColor:false,
            minSpotColor: false,
            spotColor:false,
            lineWidth: 1
        });

        $('#sparkline1').sparkline([25, 23, 26, 24, 25, 32, 30, 24, 19], {
                type: 'line',
                width: "100%",
                height: '165',
                chartRangeMax: 40,
                lineColor: '#f672a7',
                fillColor: 'rgba(246, 114, 164, 0.3)',
                composite: true,
                highlightLineColor: 'rgba(0,0,0,.1)',
                highlightSpotColor: 'rgba(0,0,0,.2)',
                maxSpotColor:false,
                minSpotColor: false,
                spotColor:false,
                lineWidth: 1
        });

        $('#sparkline2').sparkline([[70,40], [90, 50], [100, 150], [140, 80],[50,90], [80, 120], [130, 80], [90, 70],[80,50], [120, 130], [120, 100], [140, 110]], {
                type: 'bar',
                height: '165',
                barWidth: '15',
                barSpacing: '3',
                stackedBarColor: ['#7266ba', '#e3eaef']
        });
        
        $('#sparkline3').sparkline([20, 40, 30], {
                type: 'pie',
                width: '165',
                height: '165',
                sliceColors: ['#e3eaef', '#7266ba', '#f672a7']
        });

        $('#sparkline4').sparkline([0, 23, 43, 35, 44, 45, 56, 37, 40], {
            type: 'line',
            width:'100%',
            height: '165',
            chartRangeMax: 50,
            lineColor: '#3bafda',
            fillColor: 'transparent',
            highlightLineColor: 'rgba(0,0,0,.1)',
            highlightSpotColor: 'rgba(0,0,0,.2)'
        });

        $('#sparkline4').sparkline([25, 23, 26, 24, 25, 32, 30, 24, 19], {
            type: 'line',
            width:'100%',
            height: '165',
            chartRangeMax: 40,
            lineColor: '#5d9cec',
            fillColor: 'transparent',
            composite: true,
            highlightLineColor: 'rgba(0,0,0,1)',
            highlightSpotColor: 'rgba(0,0,0,1)'
        });

        $('#sparkline6').sparkline([3, 6, 7, 8, 6, 4, 7, 10, 12, 7, 4, 9, 12, 13, 11, 12], {
            type: 'bar',
            height: '165',
            barWidth: '10',
            barSpacing: '3',
            barColor: '#6c757d'
        });

        $('#sparkline6').sparkline([3, 6, 7, 8, 6, 4, 7, 10, 12, 7, 4, 9, 12, 13, 11, 12], {
            type: 'line',
            width:'100%',
            height: '165',
            lineColor: '#fb6d9d',
            fillColor: 'transparent',
            composite: true,
            highlightLineColor: 'rgba(0,0,0,.1)',
            highlightSpotColor: 'rgba(0,0,0,.2)'
        });
    };
    
    DrawMouseSpeed = function () {
        var mrefreshinterval = 500; // update display every 500ms
        var lastmousex=-1;
        var lastmousey=-1;
        var lastmousetime;
        var mousetravel = 0;
        var mpoints = [];
        var mpoints_max = 30;
        $('html').mousemove(function(e) {
            var mousex = e.pageX;
            var mousey = e.pageY;
            if (lastmousex > -1) {
                mousetravel += Math.max( Math.abs(mousex-lastmousex), Math.abs(mousey-lastmousey) );
            }
            lastmousex = mousex;
            lastmousey = mousey;
        });
        var mdraw = function() {
            var md = new Date();
            var timenow = md.getTime();
            if (lastmousetime && lastmousetime!=timenow) {
                var pps = Math.round(mousetravel / (timenow - lastmousetime) * 1000);
                mpoints.push(pps);
                if (mpoints.length > mpoints_max)
                    mpoints.splice(0,1);
                mousetravel = 0;
                $('#sparkline5').sparkline(mpoints, {
                    tooltipSuffix: ' pixels per second',
                    type: 'line',
                    width:'100%',
                    height: '165',
                    chartRangeMax: 50,
                    lineColor: '#7266ba',
                    fillColor: 'rgba(114,102,186,0.3)',
                    highlightLineColor: 'rgba(24,147,126,.1)',
                    highlightSpotColor: 'rgba(24,147,126,.2)'
                });
            }
            lastmousetime = timenow;
            setTimeout(mdraw, mrefreshinterval);
        }
        // We could use setInterval instead, but I prefer to do it this way
        setTimeout(mdraw, mrefreshinterval);
    };

DrawSparkline();
DrawMouseSpeed();

var resizeChart;

$(window).resize(function(e) {
    clearTimeout(resizeChart);
        resizeChart = setTimeout(function() {
            DrawSparkline();
            DrawMouseSpeed();
        }, 300);
    });
});

jQuery(document).ready(function($) {
    $('.counter').counterUp({
        delay: 100,
        time: 1200
    });
});

    