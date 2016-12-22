$(document).ready(function () {

	// Get latest successful login
	if ($.cookie('email') !== null) {
		$('#email').val($.cookie('email'));
	}

	// Check is user Authorized
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

	function ripple(elem, e) {
		$(".ripple").remove();
		var elTop = elem.offset().top,
			elLeft = elem.offset().left,
			x = e.pageX - elLeft,
			y = e.pageY - elTop;
		var $ripple = $("<div class='ripple'></div>");
		$ripple.css({ top: y, left: x });
		elem.append($ripple);
	};

	$(document).on("click", ".button-submit", function (e) {
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
	});

	function AuthorizationCompleted(isSuccessfully) {
		if (isSuccessfully) {
			$(".login-error").hide(100);
			$(".login-github").delay(100).show(100);
			$(".button-submit").addClass("success");
			$.cookie('email', $('#email').val());
			$('#password').val('');
			ShowRooms();

			// TODO: find full screen button and move to change view
			setTimeout(function () { ChangeView('.app'); }, 400);
			
			/*setTimeout(function () {
				$app.show();
				$app.css("top");
				$app.addClass("active");
			}, submitPhase2 - 70);
			setTimeout(function () {
				$login.hide();
				$login.addClass("inactive");
				animating = false;
				$(".button-submit").removeClass("success processing");
			}, submitPhase2);*/
		} else {
			$(".login-github").hide(100);
			$(".login-error").delay(100).show(100);
			$(".button-submit").removeClass("processing");
		}
		animating = false;
	}

	function ChangeView(to) {
		var currentView = $('.active');
		var nextView = $(to);

		nextView.delay(100).show();
		nextView.delay(100).css("top");
		nextView.delay(100).addClass("active");

		currentView.delay(200).hide();
		currentView.delay(200).addClass("inactive");
		$(".button-submit").delay(200).removeClass("success processing");
		animating = false;
	}

	function ShowRooms() {
		$.ajax({
			type: "POST",
			url: "/Home/GetRooms"
		}).done(function (result) {
			$('.rooms-list').html(result);
		});
	}

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

});