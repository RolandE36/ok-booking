var animating = false; // This property may be removed after spinner implementation

$(document).ready(function () {
	BindEvents();
	ShowLatestSuccessfulLogin();
	CheckIsUserAlreadyAuthorized();
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

// Show list of available offices
function ShowOffices() {
	if($('.window').hasClass('open-menu')) return;

	$.ajax({
		type: "POST",
		url: "/Home/GetOffices"
	}).done(function (result) {
		$('.view').html(result);
		setTimeout(function () { $('.view').addClass('active'); }, 100);
	});
}

function AddOfficeToFavourites(email) {
	if ($('.window').hasClass('open-menu')) return;

	$.ajax({
		type: "POST",
		url: "/Home/AddOfficeToFavourites",
		data: {
			email: email
		}
	}).done(function (result) {
	});
}

// Show list of available rooms
function ShowRooms(email) {
	if ($('.window').hasClass('open-menu')) return;

	$.ajax({
		type: "POST",
		url: "/Home/GetRooms",
		data: {
			email: email
		}
	}).done(function (result) {
		$('.view').html(result);
		setTimeout(function () { $('.view').addClass('active'); }, 100);
	});
}

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
		$(".login-error").hide(100);
		$(".login-github").delay(100).show(100);
		$(".button-submit").addClass("success");
		$.cookie('email', $('#email').val());
		$.cookie('cbRememberMe', $('#cbRememberMe').is(':checked'));
		$('#password').val('');
		ShowOffices();
	} else {
		// actions if any errors appear during authorization:
		$(".login-github").hide(100);
		$(".login-error").delay(100).show(100);
		$(".button-submit").removeClass("processing");
	}
	animating = false;
}