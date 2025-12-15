window.BlazorDropSelect = (() => {
  const handlers = new Map();

  function getElementSafe(id) {
    const el = document.getElementById(id);
    if (!el) {
      console.warn(`BlazorDropSelect: element not found: ${id}`);
    }
    return el;
  }

  function makeKey(type, id) {
    return `BlazorDropSelect:${type}:${id}`;
  }

  function setCleanup(key, fn) {
    runCleanup(key);
    handlers.set(key, fn);
  }

  function runCleanup(key) {
    const cleanup = handlers.get(key);
    if (cleanup) {
      cleanup();
      handlers.delete(key);
    }
  }

  function initInputHandler(
    dotNetHelper,
    elementId,
    delay,
    clickOutsideSelectorId
  ) {
    const input = getElementSafe(elementId);
    if (!input) return;

    let debounceTimeout;

    const onInput = () => {
      clearTimeout(debounceTimeout);
      debounceTimeout = setTimeout(() => {
        try {
          dotNetHelper.invokeMethodAsync(
            "UpdateSearchListAfterInputAsync",
            clickOutsideSelectorId
          );
        } catch {
          /* component disposed */
        }
      }, delay);
    };

    input.addEventListener("input", onInput);

    setCleanup(makeKey("input", elementId), () => {
      clearTimeout(debounceTimeout);
      input.removeEventListener("input", onInput);
    });
  }

  function unregisterInputHandler(elementId) {
    runCleanup(makeKey("input", elementId));
  }

  function registerClickOutsideHandler(dotNetHelper, elementId) {
    const root = getElementSafe(elementId);
    if (!root) return;

    const onClick = (event) => {
      if (root.contains(event.target)) return;

      try {
        dotNetHelper.invokeMethodAsync("OnClickOutsideAsync", elementId);
      } catch {
        /* component disposed */
      }
    };

    document.addEventListener("click", onClick);

    setCleanup(makeKey("click", elementId), () => {
      document.removeEventListener("click", onClick);
    });
  }

  function unregisterClickOutsideHandler(elementId) {
    runCleanup(makeKey("click", elementId));
  }

  function registerScrollHandler(
    dotNetHelper,
    containerId,
    methodName,
    threshold = 50
  ) {
    const container = getElementSafe(containerId);
    if (!container) return;

    let locked = false;

    const onScroll = () => {
      if (locked) return;

      if (
        container.scrollTop + container.clientHeight >=
        container.scrollHeight - threshold
      ) {
        locked = true;

        try {
          const result = dotNetHelper.invokeMethodAsync(methodName);
          if (result?.finally) {
            result.finally(() => (locked = false));
          } else {
            locked = false;
          }
        } catch {
          locked = false;
        }
      }
    };

    container.addEventListener("scroll", onScroll);

    setCleanup(makeKey("scroll", containerId), () => {
      container.removeEventListener("scroll", onScroll);
    });
  }

  function unregisterScrollHandler(containerId) {
    runCleanup(makeKey("scroll", containerId));
  }

  return {
    initInputHandler,
    unregisterInputHandler,

    registerClickOutsideHandler,
    unregisterClickOutsideHandler,

    registerScrollHandler,
    unregisterScrollHandler,
  };
})();
