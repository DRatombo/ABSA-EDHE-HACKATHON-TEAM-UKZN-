// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// =============================================================
// VERA - SITE INTERACTIONS
// =============================================================

document.addEventListener("DOMContentLoaded", function () {
	initScrollReveal();
	initStepAccordions();
	initCheckCards();
	initFundingGapCalculator();
});


// -----------------------------------------------------------
// SCROLL REVEAL
// Fades/slides content in as it enters the viewport.
// Targets existing card/section classes - no markup changes needed.
// -----------------------------------------------------------

function initScrollReveal() {

	var targets = document.querySelectorAll(
		".how-step, .how-check-card, .how-summary-card, .how-example-card, " +
		".how-example-copy, .how-stat-card, .audience-card, .home-journey-card"
	);

	if (!targets.length) return;

	targets.forEach(function (el) {
		el.classList.add("vera-reveal");
	});

	if (!("IntersectionObserver" in window)) {
		targets.forEach(function (el) { el.classList.add("vera-reveal-visible"); });
		return;
	}

	var observer = new IntersectionObserver(function (entries) {
		entries.forEach(function (entry) {
			if (entry.isIntersecting) {
				entry.target.classList.add("vera-reveal-visible");
				observer.unobserve(entry.target);
			}
		});
	}, { threshold: 0.12, rootMargin: "0px 0px -40px 0px" });

	targets.forEach(function (el) { observer.observe(el); });
}


// -----------------------------------------------------------
// STEP ACCORDIONS
// Turns any ".how-step-list" into a click-to-expand accordion.
// First step in each list opens by default.
// -----------------------------------------------------------

function initStepAccordions() {

	var lists = document.querySelectorAll(".how-step-list");

	lists.forEach(function (list) {

		var steps = list.querySelectorAll(".how-step");

		steps.forEach(function (step, index) {

			step.setAttribute("tabindex", "0");
			step.setAttribute("role", "button");
			step.setAttribute("aria-expanded", index === 0 ? "true" : "false");

			// Inject a chevron indicator if one isn't already there
			if (!step.querySelector(".how-step-chevron")) {
				var chevron = document.createElement("i");
				chevron.className = "bi bi-chevron-down how-step-chevron";
				var copy = step.querySelector(".how-step-copy");
				if (copy) copy.appendChild(chevron);
			}

			if (index === 0) {
				step.classList.add("is-open");
			}

			function toggle() {
				var isOpen = step.classList.contains("is-open");

				steps.forEach(function (s) {
					s.classList.remove("is-open");
					s.setAttribute("aria-expanded", "false");
				});

				if (!isOpen) {
					step.classList.add("is-open");
					step.setAttribute("aria-expanded", "true");
				}
			}

			step.addEventListener("click", toggle);
			step.addEventListener("keydown", function (e) {
				if (e.key === "Enter" || e.key === " ") {
					e.preventDefault();
					toggle();
				}
			});
		});
	});
}


// -----------------------------------------------------------
// CHECK CARDS
// Lets visitors "mark as explored" on benefit/check cards, with
// a small live progress counter injected above each grid.
// -----------------------------------------------------------

function initCheckCards() {

	var grids = document.querySelectorAll(".how-check-grid");

	grids.forEach(function (grid) {

		var cards = grid.querySelectorAll(".how-check-card");
		if (!cards.length) return;

		var progress = document.createElement("div");
		progress.className = "how-check-progress";
		progress.innerHTML =
			'<span class="how-check-progress-fill"></span>' +
			'<small></small>';
		grid.parentNode.insertBefore(progress, grid);

		var fill = progress.querySelector(".how-check-progress-fill");
		var label = progress.querySelector("small");

		function updateProgress() {
			var opened = grid.querySelectorAll(".how-check-card.is-explored").length;
			var pct = Math.round((opened / cards.length) * 100);
			fill.style.width = pct + "%";
			label.textContent = opened === 0
				? "Tap a card to explore"
				: opened + " of " + cards.length + " explored";
		}

		cards.forEach(function (card) {

			card.setAttribute("tabindex", "0");
			card.setAttribute("role", "button");

			function toggle() {
				card.classList.toggle("is-explored");
				updateProgress();
			}

			card.addEventListener("click", toggle);
			card.addEventListener("keydown", function (e) {
				if (e.key === "Enter" || e.key === " ") {
					e.preventDefault();
					toggle();
				}
			});
		});

		updateProgress();
	});
}


// -----------------------------------------------------------
// FUNDING GAP CALCULATOR (For SMEs page)
// -----------------------------------------------------------

function initFundingGapCalculator() {

	var poInput = document.getElementById("gapCalcPO");
	var costInput = document.getElementById("gapCalcCostPct");
	var contribInput = document.getElementById("gapCalcContribPct");

	if (!poInput || !costInput || !contribInput) return;

	var poValueLabel = document.getElementById("gapCalcPOValue");
	var costValueLabel = document.getElementById("gapCalcCostValue");
	var contribValueLabel = document.getElementById("gapCalcContribValue");

	var costOut = document.getElementById("gapCalcCostOut");
	var contribOut = document.getElementById("gapCalcContribOut");
	var gapOut = document.getElementById("gapCalcGapOut");
	var barFill = document.getElementById("gapCalcBarFill");

	function formatRand(value) {
		return "R" + Math.round(value).toLocaleString("en-ZA");
	}

	function recalculate() {

		var po = parseFloat(poInput.value) || 0;
		var costPct = parseFloat(costInput.value) || 0;
		var contribPct = parseFloat(contribInput.value) || 0;

		var fulfilmentCost = po * (costPct / 100);
		var contribution = po * (contribPct / 100);
		var gap = Math.max(fulfilmentCost - contribution, 0);

		poValueLabel.textContent = formatRand(po);
		costValueLabel.textContent = costPct + "%";
		contribValueLabel.textContent = contribPct + "%";

		costOut.textContent = formatRand(fulfilmentCost);
		contribOut.textContent = formatRand(contribution);
		gapOut.textContent = formatRand(gap);

		var gapPct = fulfilmentCost > 0 ? Math.min((gap / fulfilmentCost) * 100, 100) : 0;
		if (barFill) barFill.style.width = gapPct + "%";
	}

	[poInput, costInput, contribInput].forEach(function (input) {
		input.addEventListener("input", recalculate);
	});

	recalculate();
}
