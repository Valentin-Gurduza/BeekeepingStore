document.addEventListener("DOMContentLoaded", function () {
    // Add event listener for navigation links
    var navLinks = document.querySelectorAll("nav ul li a");
    navLinks.forEach(function (link) {
        link.addEventListener("click", function (event) {
            event.preventDefault();
            var targetId = this.getAttribute("href").substring(1);
            var targetElement = document.getElementById(targetId);
            if (targetElement) {
                window.scrollTo({
                    top: targetElement.offsetTop,
                    behavior: "smooth"
                });
            }
        });
    });

    // Add event listener for jumbotron button
    var jumbotronButton = document.querySelector(".jumbotron button");
    if (jumbotronButton) {
        jumbotronButton.addEventListener("click", function () {
            alert("Welcome to the Beekeeping Store!");
        });
    }
});
