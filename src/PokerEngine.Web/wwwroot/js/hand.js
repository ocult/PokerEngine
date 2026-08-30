const handForm = document.querySelector('#hand-form');
const handResult = document.querySelector('#hand-result');

function renderError(message) {
  handResult.innerHTML = '';
  handResult.classList.add('is-error');
  handResult.appendChild(createHandSummaryBlock('My hand', '-', message, ''));
  handResult.appendChild(createResultRawBlock('API Response', message));
}

function renderSuccess(payload) {
  handResult.innerHTML = '';
  handResult.classList.remove('is-error');
  handResult.appendChild(createHandSummaryBlock(
    'My hand',
    payload.ranking ?? '-',
    payload.description ?? '-',
    payload.cards ?? ''
  ));
  handResult.appendChild(createResultRawBlock('API Response', JSON.stringify(payload, null, 2)));
}

if (handForm) {
  handForm.addEventListener('submit', async (event) => {
    event.preventDefault();

    const cards = document.querySelector('#cards').value;
    const response = await fetch('/api/hand', {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams({ cards })
    });

    const payload = await response.json();

    if (!response.ok) {
      renderError(payload.error ?? 'Fail to evaluate the hand.');
      return;
    }

    renderSuccess(payload);
  });
}
