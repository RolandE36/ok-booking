var animating = false; // This property may be removed after spinner implementation

$(document).ready(function () {
	BindEvents();
	ShowLatestSuccessfulLogin();
	CheckIsUserAlreadyAuthorized();

	// Set current device time zone settings
	$.cookie('TIMEZONEOFFSET', new Date().getTimezoneOffset());
});

// Bind default functions for all inputs, buttons, fields, etc...
function BindEvents() {
	// After login button click we should try to authorize user
	$(document).on("click", ".button-submit", function () { Authorization(); });

	// After ENTER button click (on password field) we should try to to authorize user
	$(document).on("keypress", "#password", function (e) {
		if (e.which == 13) { Authorization(); return false; } // if we press ENTER button (13) than try to authorize user and prevent default action (return false;)
		return true; // user default action in case of other button pressed (return true;)
	});

	// toggle menu
	$(document).on("click", '.menu-btn', function () { $('.window').toggleClass('open-menu'); });
	//$(document).on("swiperight", '.window', function () { alert(1); if (!$('.window').hasClass('open-menu') && $('.login').length == 0) $('.window').toggleClass('open-menu'); });
	//$(document).on("swipeleft", '.window', function () { alert(2); if ($('.window').hasClass('open-menu')) $('.window').toggleClass('open-menu'); });
}

// Get latest successful login (remember me functionality)
function ShowLatestSuccessfulLogin() {
	if ($.cookie('email') !== null && $.cookie('cbRememberMe') == "true") {
		$('#email').val($.cookie('email'));
		$("#cbRememberMe").prop("checked", true);
	}
}

// Check is user Authorized
function CheckIsUserAlreadyAuthorized() {
	$.ajax({
		type: "POST",
		url: "/Home/IsAuthorized"
	}).done(function (result) {
		if (result == 'True') {
			AuthorizationCompleted(true);
		}
	});
}

// Show progress and hide menu
function ShowProgress() {
	$(".progress").show(); 
	$('.window').removeClass('open-menu');
}

// Show list of available offices
function ShowOffices() {
	$.ajax({
		type: "POST",
		url: "/Home/GetOffices",
		beforeSend: ShowProgress,
		complete: function () { $(".progress").hide(); }
	}).done(RenderView);
}

// Show offices is no favorite office selected
// In other case show all romms for favorite office
function ShowOfficesOrRooms() {
	$.ajax({
		type: "POST",
		url: "/Home/GetOfficesOrRooms",
		beforeSend: ShowProgress,
		complete: function () { $(".progress").hide(); }
	}).done(function (result) {
		if ($(".button-submit").length > 0) {
			$('#password').val('');
			$(".login-error").hide(100);
			$(".login-github").delay(100).show(100);
			$(".button-submit").addClass("success");
			setTimeout(function() { RenderView(result); }, 400);
		} else {
			RenderView(result);
		}
	});
}

function SetFavouriteOffice(email) {
	if ($('.window').hasClass('open-menu')) return;

	$(this.event.srcElement).toggleClass("fav-star-active");
	$('.fav-star-active').not(this.event.srcElement).removeClass("fav-star-active");

	$.ajax({
		type: "POST",
		url: "/Home/SetFavouriteOffice",
		data: {
			email: email
		}
	}).done(function (result) {
	});

	this.event.preventDefault();
	this.event.stopPropagation();
}

function ToggleFavouriteRoom(email) {
	if ($('.window').hasClass('open-menu')) return;

	$(this.event.srcElement).toggleClass("fav-star-active");

	$.ajax({
		type: "POST",
		url: "/Home/ToggleFavouriteRoom",
		data: {
			email: email
		}
	}).done(function (result) {
	});

	this.event.preventDefault();
	this.event.stopPropagation();
}

// Show list of available rooms
function ShowRooms(email) {
	if ($('.window').hasClass('open-menu')) return;

	$.ajax({
		type: "POST",
		url: "/Home/GetRooms",
		data: {
			email: email
		},
		beforeSend: ShowProgress,
		complete: function () { $(".progress").hide(); }
	}).done(RenderView);
}

// Show booking page
function ShowBooking(name, email, startAvailableTime, endAvailableTime) {
	if ($('.window').hasClass('open-menu')) return;

	$.ajax({
		type: "POST",
		url: "/Home/GetBooking",
		data: {
			name: name,
			email: email,
			startAvailableTime: startAvailableTime,
			endAvailableTime: endAvailableTime
		}
	}).done(RenderView);
}

function BookNow(email, name) {
	// Get Values
	var startTimeArr = $('.start-time').val().split(':');
	var stHours = parseInt(startTimeArr[0]);
	var stMin = parseInt(startTimeArr[1]);
	var endTimeArr = $('.end-time').val().split(':');
	var endHours = parseInt(endTimeArr[0]);
	var endMin = parseInt(endTimeArr[1]);
	
	// Booking
	$.ajax({
		type: "POST",
		url: "/Home/BookNow",
		data: {
			email: email,
			subject: name,
			start: stHours * 60 + stMin,
			end: endHours * 60 + endMin
		},
		beforeSend: ShowProgress,
		complete: function () { $(".progress").hide(); }
	}).done(function (msg) {
		alert(msg);
	});
}

// When session is expired, user redirects to Login Page
var sessionExpirationTimeout = null;
$('*').bind('click', function () {
	if (sessionExpirationTimeout != null) clearTimeout(sessionExpirationTimeout);
	sessionExpirationTimeout = setTimeout(LogOut, 1000 * 60 * 15);
});

// Email and password validation
function Authorization() {
	if (animating) return;
	animating = true;
	$(".button-submit").addClass("processing");

	$.ajax({
		type: "POST",
		url: "/Home/Login",
		data: {
			email: $('#email').val(),
			password: $('#password').val()
		}
	}).done(function (result) {
		AuthorizationCompleted(result == 'True');
	});
}

// Default actions after authorization check
function AuthorizationCompleted(isSuccessfully) {
	if (isSuccessfully) {
		// actions if user authorized successfully:
		$.cookie('email', $('#email').val());
		$.cookie('cbRememberMe', $('#cbRememberMe').is(':checked'));
		ShowOfficesOrRooms();
	} else {
		// actions if any errors appear during authorization:
		$(".login-github").hide(100);
		$(".login-error").delay(100).show(100);
		$(".button-submit").removeClass("processing");
	}
	animating = false;
}

function LogOut() {
	$.ajax({
		type: "POST",
		url: "/Home/LogOut",
		beforeSend: ShowProgress,
		complete: function () { $(".progress").hide(); }
	}).done(RenderView);
}

// show settings page
function ShowSettings() {
	$.ajax({
		type: "POST",
		url: "/Home/GetSettings",
		beforeSend: ShowProgress,
		complete: function () { $(".progress").hide(); }
	}).done(RenderView);
}

function RenderView(result) {
	$('.view').html(result);
	$('.window').removeClass('open-menu');
	setTimeout(function () { $('.view').addClass('active'); }, 100); // Trigger animation
}