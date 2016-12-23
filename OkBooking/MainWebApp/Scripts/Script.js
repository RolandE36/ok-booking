$(document).ready(function () {

	// Get latest successful login
	if ($.cookie('email') !== null) {
		$('#email').val($.cookie('email'));
	}

	// Check is user Authorized
	// TODO: do this on C# side.
	$.ajax({
		type: "POST",
		url: "/Home/IsAuthorized"
	}).done(function (result) {
		if (result == 'True') {
			AuthorizationCompleted(true);
		}
	});

	var animating = false,
		submitPhase2 = 400,
		logoutPhase1 = 800,
		$login = $(".login"),
		$app = $(".app");

	// Login actions
	$(document).on("click", ".button-submit", function() { Authorization(); });
	$(document).on("keypress", "#password", function (e) {
		if (e.which == 13) {
			Authorization();
			return false;
		}
	});

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

	function AuthorizationCompleted(isSuccessfully) {
		if (isSuccessfully) {
			$(".login-error").hide(100);
			$(".login-github").delay(100).show(100);
			$(".button-submit").addClass("success");
			$.cookie('email', $('#email').val());
			$('#password').val('');
			ShowOffices();
		} else {
			$(".login-github").hide(100);
			$(".login-error").delay(100).show(100);
			$(".button-submit").removeClass("processing");
		}
		animating = false;
	}
	
	/*
	$(document).on("click", ".app__logout", function (e) {
		if (animating) return;
		$(".ripple").remove();
		animating = true;
		var that = this;
		$(that).addClass("clicked");
		setTimeout(function () {
			$app.removeClass("active");
			$login.show();
			$login.css("top");
			$login.removeClass("inactive");
		}, logoutPhase1 - 120);
		setTimeout(function () {
			$app.hide();
			animating = false;
			$(that).removeClass("clicked");
		}, logoutPhase1);
	});
	*/
});

function ShowOffices() {
	$.ajax({
		type: "POST",
		url: "/Home/GetOffices"
	}).done(function (result) {
		$('.window').html(result);
		setTimeout(function () { $('.view').addClass('active'); }, 100);
	});
}

function ShowRooms(email) {
	$.ajax({
		type: "POST",
		url: "/Home/GetRooms",
		data: {
			email: email
		}
	}).done(function (result) {
		$('.window').html(result);
		setTimeout(function () { $('.view').addClass('active'); }, 100);
	});
}