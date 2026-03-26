(function () {
    const searchInput = document.getElementById("navbarSearchInput");
    const dropdown = document.getElementById("navbarSearchDropdown");
    const stateEl = document.getElementById("navbarSearchState");
    const resultsEl = document.getElementById("navbarSearchResults");
    const allLink = document.getElementById("navbarSearchAllLink");

    if (!searchInput || !dropdown || !stateEl || !resultsEl || !allLink) {
        return;
    }

    const debounceMs = 300;
    const minChars = 2;
    const pageSize = 6;
    let timerId = null;
    let activeRequest = null;
    let latestQuery = "";

    function escapeHtml(value) {
        return value
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function showDropdown() {
        dropdown.classList.remove("d-none");
    }

    function hideDropdown() {
        dropdown.classList.add("d-none");
    }

    function setState(message, showState) {
        stateEl.textContent = message;
        stateEl.classList.toggle("d-none", !showState);
    }

    function clearResults() {
        resultsEl.innerHTML = "";
    }

    function renderResults(data, query) {
        clearResults();
        if (!data || data.length === 0) {
            setState("No users found.", true);
            allLink.classList.remove("d-none");
            allLink.href = `/Search?query=${encodeURIComponent(query)}&page=1&pageSize=10`;
            return;
        }

        setState("", false);
        const items = data.map((user) => {
            const imageUrl = user.profileImage && user.profileImage.trim() !== "" ? user.profileImage : "/images/user.jpg";
            const safeName = escapeHtml(user.userName || "Unknown");
            return `
                <a class="search-nav-result-item" href="/Search?query=${encodeURIComponent(query)}&page=1&pageSize=10">
                    <img src="${imageUrl}" alt="${safeName}" class="rounded-circle" />
                    <span>${safeName}</span>
                </a>
            `;
        }).join("");

        resultsEl.innerHTML = items;
        allLink.classList.remove("d-none");
        allLink.href = `/Search?query=${encodeURIComponent(query)}&page=1&pageSize=10`;
    }

    async function loadResults(query) {
        if (activeRequest) {
            activeRequest.abort();
        }

        activeRequest = new AbortController();
        showDropdown();
        clearResults();
        setState("Searching...", true);
        allLink.classList.add("d-none");

        try {
            const response = await fetch(`/Search/Users?query=${encodeURIComponent(query)}&page=1&pageSize=${pageSize}`, {
                method: "GET",
                credentials: "same-origin",
                signal: activeRequest.signal
            });

            if (!response.ok) {
                setState("Could not load search results.", true);
                return;
            }

            const payload = await response.json();
            if (query !== latestQuery) {
                return;
            }

            renderResults(payload.data || [], query);
        } catch (error) {
            if (error.name !== "AbortError") {
                setState("Could not load search results.", true);
            }
        }
    }

    searchInput.addEventListener("input", (event) => {
        const query = event.target.value.trim();
        latestQuery = query;

        if (timerId) {
            clearTimeout(timerId);
        }

        if (query.length < minChars) {
            if (!query.length) {
                hideDropdown();
            } else {
                showDropdown();
                clearResults();
                setState(`Type at least ${minChars} characters`, true);
            }
            allLink.classList.add("d-none");
            return;
        }

        timerId = setTimeout(() => {
            loadResults(query);
        }, debounceMs);
    });

    searchInput.addEventListener("focus", () => {
        if (searchInput.value.trim().length >= minChars || stateEl.textContent) {
            showDropdown();
        }
    });

    document.addEventListener("click", (event) => {
        if (!dropdown.contains(event.target) && event.target !== searchInput) {
            hideDropdown();
        }
    });
})();
