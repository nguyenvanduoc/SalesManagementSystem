let globalSearchTimeout = null;
let currentSearchItems = [];
let searchActiveIndex = -1;

document.addEventListener('DOMContentLoaded', function () {
    const searchOverlay = document.getElementById('globalSearchOverlay');
    const searchInput = document.getElementById('globalSearchInput');
    const btnCloseSearch = document.getElementById('btnCloseSearch');
    const searchResults = document.getElementById('globalSearchResults');

    if (!searchOverlay || !searchInput) return;

    // Toggle Modal on Ctrl+Q
    document.addEventListener('keydown', function (e) {
        if (e.ctrlKey && (e.key === 'q' || e.key === 'Q')) {
            e.preventDefault();
            toggleSearchModal();
        }
    });

    // Close on Escape or click outside
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && searchOverlay.classList.contains('active')) {
            closeSearchModal();
        }
    });

    searchOverlay.addEventListener('click', function (e) {
        if (e.target === searchOverlay) {
            closeSearchModal();
        }
    });

    btnCloseSearch.addEventListener('click', closeSearchModal);

    // Input keyboard navigation & typing
    searchInput.addEventListener('keydown', function (e) {
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            navigateResults(1);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            navigateResults(-1);
        } else if (e.key === 'Enter') {
            e.preventDefault();
            if (searchActiveIndex >= 0 && searchActiveIndex < currentSearchItems.length) {
                openMenuItem(currentSearchItems[searchActiveIndex]);
            }
        }
    });

    searchInput.addEventListener('input', function (e) {
        const query = e.target.value.trim();
        
        clearTimeout(globalSearchTimeout);
        globalSearchTimeout = setTimeout(() => {
            performSearch(query);
        }, 300);
    });

    function toggleSearchModal() {
        if (searchOverlay.classList.contains('active')) {
            closeSearchModal();
        } else {
            searchOverlay.classList.add('active');
            searchInput.value = '';
            searchResults.innerHTML = '';
            currentSearchItems = [];
            searchActiveIndex = -1;
            performSearch(''); // Load all by default
            setTimeout(() => searchInput.focus(), 100);
        }
    }

    function closeSearchModal() {
        searchOverlay.classList.remove('active');
        searchInput.blur();
    }

    function performSearch(query) {
        // Assume API endpoint is /Home/SearchMenu
        const url = '/Home/SearchMenu?q=' + encodeURIComponent(query);
        
        fetch(url)
            .then(res => res.json())
            .then(data => {
                if (data.error) {
                    console.error('Search error:', data.error);
                    return;
                }
                
                currentSearchItems = data || [];
                renderResults(currentSearchItems);
            })
            .catch(err => {
                console.error('Search request failed:', err);
            });
    }

    function renderResults(items) {
        searchResults.innerHTML = '';
        if (items.length === 0) {
            searchResults.innerHTML = '<li class="text-muted text-center py-3">Không tìm thấy kết quả phù hợp.</li>';
            searchActiveIndex = -1;
            return;
        }

        items.forEach((item, index) => {
            const li = document.createElement('li');
            li.dataset.index = index;
            
            const titleSpan = document.createElement('span');
            titleSpan.className = 'search-item-title';
            titleSpan.textContent = item.TenManHinh;
            
            const breadSpan = document.createElement('span');
            breadSpan.className = 'search-item-breadcrumb';
            breadSpan.textContent = item.Breadcrumb;

            li.appendChild(titleSpan);
            li.appendChild(breadSpan);

            li.addEventListener('click', () => {
                openMenuItem(item);
            });
            
            li.addEventListener('mousemove', () => {
                if (searchActiveIndex !== index) {
                    searchActiveIndex = index;
                    updateActiveItem();
                }
            });

            searchResults.appendChild(li);
        });

        // Auto select first item
        searchActiveIndex = 0;
        updateActiveItem();
    }

    function navigateResults(direction) {
        if (currentSearchItems.length === 0) return;
        
        searchActiveIndex += direction;
        
        if (searchActiveIndex < 0) {
            searchActiveIndex = currentSearchItems.length - 1;
        } else if (searchActiveIndex >= currentSearchItems.length) {
            searchActiveIndex = 0;
        }
        
        updateActiveItem();
    }

    function updateActiveItem() {
        const lis = searchResults.querySelectorAll('li');
        lis.forEach((li, idx) => {
            if (idx === searchActiveIndex) {
                li.classList.add('active');
                li.scrollIntoView({ block: 'nearest' });
            } else {
                li.classList.remove('active');
            }
        });
    }

    function openMenuItem(item) {
        closeSearchModal();
        if (item.DuongDan && item.DuongDan !== '#') {
            if (typeof TabManager !== 'undefined' && TabManager.openTab) {
                var key = item.TenController || 'Tab_' + item.IDManHinh;
                TabManager.openTab(key, item.TenManHinh, item.DuongDan);
            } else if (typeof loadData === 'function') {
                loadData(item.DuongDan);
            } else {
                window.location.href = item.DuongDan;
            }
        }
    }
});
