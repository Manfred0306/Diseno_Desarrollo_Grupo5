const slideLeft = {
  reset: true,
  duration: "1500",
  distance: "25px",
  delay:250,
  origin:'left',
  easing: 'cubic-bezier(0.5, 0, 0, 0.5)',
  opacity: 0,
};

const slideRight = {
  reset: true,
  duration: "1500",
  distance: "25px",
  delay:250,
  origin:'right',
  easing: 'cubic-bezier(0.5, 0, 0, 0.5)',
  opacity: 0,
};

const slideUp = {
  reset: true,
  duration: "1500",
  distance: "150px",
  delay:250,
  origin: 'bottom',
  easing: 'cubic-bezier(0.5, 0, 0, 0.5)',
  opacity: 0,
};

const slideUpCopyRight = {
  reset: true,
  duration: "1500",
  distance: "10px",
  delay:250,
  origin: 'bottom',
  easing: 'cubic-bezier(0.5, 0, 0, 0.5)',
  opacity: 0,
};

let nodeArrayLeft = [
  document.querySelector('.hero__image'),
  document.querySelector('.about__info'),
  document.querySelector('.footer__logo'),
];

let nodeArrayRight = [
  document.querySelector('.hero__info'),
  document.querySelector('.about__image'),
  document.querySelector('.f__faq'),
];

let nodeSlideUp = [
  document.querySelector('.promise__image'),
  document.querySelector('.promise__info'),
  document.querySelector('.menu__title'),
  document.querySelector('.btn__menu'),
  document.querySelector('.booking__title'),
  document.querySelector('.booking__text'),
  document.querySelector('.blog__title'),
  document.querySelector('.f__about-us'),
  document.querySelector('.f__contact'),
];

const nodeListMenu = document.querySelectorAll('.card');
const nodeListBlog = document.querySelectorAll('.entry');
const nodeListBooking = document.querySelectorAll('.btn__place');

// Si la página marca que no quiere animaciones (por ejemplo Usuarios / Roles), no inicializar.
if (document.querySelector('[data-no-reveal="true"]')) {
  // no-op
} else {
  ScrollReveal().reveal(nodeArrayLeft, slideLeft);
  ScrollReveal().reveal(nodeArrayRight, slideRight);
  ScrollReveal().reveal(nodeSlideUp, slideUp);
  ScrollReveal().reveal(nodeListMenu, slideUp);
  ScrollReveal().reveal(nodeListBlog, slideUp);
  ScrollReveal().reveal(nodeListBooking, slideUp);
  ScrollReveal().reveal('.footer__copyright', slideUpCopyRight);
}