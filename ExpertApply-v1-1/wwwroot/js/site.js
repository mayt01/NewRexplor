document.addEventListener('DOMContentLoaded', function () {
    const faqToggles = document.querySelectorAll('.faq-toggle');

    faqToggles.forEach(toggle => {
        toggle.addEventListener('click', function () {
            const faqItem = this.closest('.faq-item');
            faqItem.classList.toggle('active');
        });
    });
});


// Show/Hide Back-to-Top Button on Scroll
window.onscroll = function () {
    var backToTopBtn = document.getElementById("backToTop");
    if (document.body.scrollTop > 300 || document.documentElement.scrollTop > 300) {
        backToTopBtn.classList.add("show");
    } else {
        backToTopBtn.classList.remove("show");
    }
};

// Smooth scroll to top
document.getElementById("backToTop").addEventListener("click", function (event) {
    event.preventDefault();
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
});


document.addEventListener('DOMContentLoaded', function () {
    const toggleButton = document.querySelector('.navbar-toggler');
    const navbarCollapse = document.querySelector('.navbar-collapse');

    toggleButton.addEventListener('click', function () {
        navbarCollapse.classList.toggle('show'); // Toggle 'show' class
    });

    document.addEventListener('click', function (e) {
        if (!toggleButton.contains(e.target) && !navbarCollapse.contains(e.target)) {
            navbarCollapse.classList.remove('show'); // Close the menu if clicked outside
        }
    });
});









