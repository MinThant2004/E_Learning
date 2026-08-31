window.opencodeModalFocus = function (element) {
    if (!element) return;
    element.focus();
    const focusables = () => element.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    const onKeydown = (e) => {
        if (e.key !== 'Tab') return;
        const list = Array.from(focusables()).filter(el => !el.disabled);
        if (list.length === 0) return;
        const first = list[0];
        const last = list[list.length - 1];
        if (e.shiftKey && document.activeElement === first) {
            e.preventDefault();
            last.focus();
        } else if (!e.shiftKey && document.activeElement === last) {
            e.preventDefault();
            first.focus();
        } else if (!element.contains(document.activeElement)) {
            e.preventDefault();
            first.focus();
        }
    };
    document._opencodeActiveTrap = onKeydown;
    document.addEventListener('keydown', onKeydown, true);
};

window.opencodeModalFocusReset = function () {
    document.removeEventListener('keydown', document._opencodeActiveTrap, true);
    document._opencodeActiveTrap = null;
    if (document.activeElement && document.activeElement.blur) {
        document.activeElement.blur();
    }
};

window.opencodeLockBodyScroll = function () {
    const scrollY = window.scrollY || document.documentElement.scrollTop;
    document.body._opencodeScrollY = scrollY;
    document.body.style.position = 'fixed';
    document.body.style.top = '-' + scrollY + 'px';
    document.body.style.left = '0';
    document.body.style.right = '0';
    document.body.style.width = '100%';
    document.body.style.overflow = 'hidden';
};

window.opencodeUnlockBodyScroll = function () {
    const scrollY = document.body._opencodeScrollY || 0;
    document.body.style.position = '';
    document.body.style.top = '';
    document.body.style.left = '';
    document.body.style.right = '';
    document.body.style.width = '';
    document.body.style.overflow = '';
    window.scrollTo(0, scrollY);
    delete document.body._opencodeScrollY;
};
