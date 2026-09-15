const button=document.querySelector('.menu-button');
const nav=document.querySelector('.nav-links');
if(button&&nav){
  const closeMenu=(restoreFocus=false)=>{
    nav.classList.remove('open');
    button.setAttribute('aria-expanded','false');
    if(restoreFocus) button.focus();
  };
  button.addEventListener('click',()=>{
    const open=nav.classList.toggle('open');
    button.setAttribute('aria-expanded',String(open));
    if(open) nav.querySelector('a')?.focus();
  });
  nav.addEventListener('click',event=>{
    if(event.target.closest('a')) closeMenu(false);
  });
  document.addEventListener('keydown',event=>{
    if(event.key==='Escape'&&nav.classList.contains('open')) closeMenu(true);
  });
}
