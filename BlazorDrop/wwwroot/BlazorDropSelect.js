window.BlazorDropSelect = (() => {
  const cleanups = new Map();
  const scrollChecks = new Map();
  const MOBILE_QUERY = "(max-width: 600px)";
  const SCROLL_THRESHOLD = 48;
  const INPUT_NAV_KEYS = ["ArrowUp", "ArrowDown", "Enter", "Escape"];
  const LIST_NAV_KEYS = ["ArrowUp", "ArrowDown", "Home", "End", "Enter"];

  function byId(id) {
    const el = document.getElementById(id);
    if (!el) {
      console.warn(`BlazorDropSelect: element not found: ${id}`);
    }
    return el;
  }

  function key(type, id) {
    return `${type}:${id}`;
  }

  function setCleanup(k, fn) {
    runCleanup(k);
    cleanups.set(k, fn);
  }

  function runCleanup(k) {
    const fn = cleanups.get(k);
    if (fn) {
      cleanups.delete(k);
      fn();
    }
  }

  function isMobile() {
    return !!(window.matchMedia && window.matchMedia(MOBILE_QUERY).matches);
  }

  function prefersReducedMotion() {
    return !!(window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches);
  }

  function isExpanded(el) {
    const value = el.getAttribute("aria-expanded");
    return value === null || value === "true";
  }

  function guardKeys(el, keys) {
    const onKeyDown = (e) => {
      if (e.key === "ArrowDown" || (isExpanded(el) && keys.includes(e.key))) {
        e.preventDefault();
      }
    };
    el.addEventListener("keydown", onKeyDown);
    return () => el.removeEventListener("keydown", onKeyDown);
  }

  function initInputHandler(dotNetHelper, elementId, delay) {
    const input = byId(elementId);
    if (!input) return;

    let timer;
    const cancel = () => clearTimeout(timer);
    const onInput = () => {
      cancel();
      timer = setTimeout(() => {
        dotNetHelper.invokeMethodAsync("UpdateSearchListAfterInputAsync").catch(() => {});
      }, delay);
    };
    const onKeyDown = (e) => {
      if (e.key === "Escape" || e.key === "Tab" || e.key === "Enter") cancel();
    };
    input.addEventListener("input", onInput);
    input.addEventListener("keydown", onKeyDown);
    input.addEventListener("blur", cancel);
    const unguard = guardKeys(input, INPUT_NAV_KEYS);

    setCleanup(key("input", elementId), () => {
      cancel();
      input.removeEventListener("input", onInput);
      input.removeEventListener("keydown", onKeyDown);
      input.removeEventListener("blur", cancel);
      unguard();
    });
  }

  function unregisterInputHandler(elementId) {
    runCleanup(key("input", elementId));
  }

  function registerKeyboardGuard(elementId) {
    const el = byId(elementId);
    if (!el) return;
    setCleanup(key("keys", elementId), guardKeys(el, LIST_NAV_KEYS));
  }

  function unregisterKeyboardGuard(elementId) {
    runCleanup(key("keys", elementId));
  }

  const clickOutsideRegistry = new Map();
  let documentClickAttached = false;

  function onDocumentClick(event) {
    clickOutsideRegistry.forEach((helper, containerId) => {
      const root = document.getElementById(containerId);
      if (!root) {
        clickOutsideRegistry.delete(containerId);
        return;
      }
      if (!root.contains(event.target)) {
        helper.invokeMethodAsync("OnClickOutsideAsync").catch(() => {});
      }
    });
  }

  function registerClickOutsideHandler(dotNetHelper, containerId) {
    clickOutsideRegistry.set(containerId, dotNetHelper);
    if (!documentClickAttached) {
      document.addEventListener("click", onDocumentClick, true);
      documentClickAttached = true;
    }
  }

  function unregisterClickOutsideHandler(containerId) {
    clickOutsideRegistry.delete(containerId);
    if (clickOutsideRegistry.size === 0 && documentClickAttached) {
      document.removeEventListener("click", onDocumentClick, true);
      documentClickAttached = false;
    }
  }

  function registerScrollHandler(dotNetHelper, containerId) {
    const container = byId(containerId);
    if (!container) return;

    let locked = false;
    let pending = false;
    const check = () => {
      if (locked) {
        pending = true;
        return;
      }
      if (container.clientHeight === 0) return;
      if (container.scrollHeight - container.scrollTop - container.clientHeight <= SCROLL_THRESHOLD) {
        locked = true;
        dotNetHelper.invokeMethodAsync("OnScrollToEndAsync").then(unlock, unlock);
      }
    };
    const unlock = () => {
      locked = false;
      if (pending) {
        pending = false;
        requestAnimationFrame(check);
      }
    };

    container.addEventListener("scroll", check, { passive: true });
    scrollChecks.set(containerId, check);
    setCleanup(key("scroll", containerId), () => {
      container.removeEventListener("scroll", check);
      scrollChecks.delete(containerId);
    });

    check();
  }

  function unregisterScrollHandler(containerId) {
    runCleanup(key("scroll", containerId));
  }

  function checkScrollEnd(containerId) {
    const check = scrollChecks.get(containerId);
    if (check) {
      requestAnimationFrame(check);
    }
  }

  function positionDropdown(anchorId, dropdownId) {
    const anchor = document.getElementById(anchorId);
    const dropdown = document.getElementById(dropdownId);
    if (!anchor || !dropdown) return;

    dropdown.classList.remove("bzd-dropdown--up");
    if (isMobile()) return;

    const rect = anchor.getBoundingClientRect();
    const spaceBelow = window.innerHeight - rect.bottom;
    const spaceAbove = rect.top;
    if (spaceBelow < dropdown.offsetHeight && spaceAbove > spaceBelow) {
      dropdown.classList.add("bzd-dropdown--up");
    }
  }

  function registerDropdownPosition(anchorId, dropdownId) {
    let frame = 0;
    const reposition = () => {
      if (frame) return;
      frame = requestAnimationFrame(() => {
        frame = 0;
        positionDropdown(anchorId, dropdownId);
      });
    };

    positionDropdown(anchorId, dropdownId);
    window.addEventListener("resize", reposition);
    window.addEventListener("scroll", reposition, true);

    setCleanup(key("pos", dropdownId), () => {
      cancelAnimationFrame(frame);
      window.removeEventListener("resize", reposition);
      window.removeEventListener("scroll", reposition, true);
    });
  }

  function unregisterDropdownPosition(dropdownId) {
    runCleanup(key("pos", dropdownId));
  }

  function scrollIntoView(elementId) {
    const el = document.getElementById(elementId);
    if (el && el.scrollIntoView) {
      el.scrollIntoView({ block: "nearest", behavior: prefersReducedMotion() ? "auto" : "smooth" });
    }
  }

  function focusElement(elementId) {
    const el = document.getElementById(elementId);
    if (el && el.focus) {
      el.focus({ preventScroll: true });
    }
  }

  return {
    initInputHandler,
    unregisterInputHandler,
    registerKeyboardGuard,
    unregisterKeyboardGuard,
    registerClickOutsideHandler,
    unregisterClickOutsideHandler,
    registerScrollHandler,
    unregisterScrollHandler,
    checkScrollEnd,
    registerDropdownPosition,
    unregisterDropdownPosition,
    scrollIntoView,
    focusElement,
  };
})();
