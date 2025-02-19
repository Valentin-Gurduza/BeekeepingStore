/*
Template Name: Minton - Responsive Bootstrap 4 Admin Dashboard
Author: CoderThemes
Website: https://coderthemes.com/
Contact: support@coderthemes.com
File: Dashboard 1
*/

$( document ).ready(function() {
    
	var DrawSparkline = function() {
			$('#sparkline1').sparkline([0, 23, 43, 35, 44, 45, 56, 37, 40], {
					type: 'line',
					width: "100%",
					height: 210,
					chartRangeMax: 50,
					lineColor: '#e3eaef',
					fillColor: 'rgba(227, 234, 239, 0.2)',
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
					height: '210',
					chartRangeMax: 40,
					lineColor: '#f06b78',
					fillColor: 'rgba(240, 107, 120, 0.3)',
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
					height: '210',
					barWidth: '15',
					barSpacing: '3',
					stackedBarColor: ['#f06b78', '#e3eaef']
			});
			
			$('#sparkline3').sparkline([20, 40, 30], {
					type: 'pie',
					width: '210',
					height: '210',
					sliceColors: ['#e3eaef', '#6c757d', '#f06b78']
			});
	
			
			};
	
	DrawSparkline();
	
	var resizeChart;

	$(window).resize(function(e) {
			clearTimeout(resizeChart);
			resizeChart = setTimeout(function() {
					DrawSparkline();
			}, 300);
	});
});