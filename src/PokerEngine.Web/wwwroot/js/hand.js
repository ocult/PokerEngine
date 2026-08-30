const handForm = document.querySelector('#hand-form');
const handResult = document.querySelector('#hand-result');
const resultCardTemplate = document.querySelector('#result-card-template');
const resultTextTemplate = document.querySelector('#result-text-template');
const resultRawTemplate = document.querySelector('#result-raw-template');

function createCardBlock(labelText, cardsText, isWide = false) {
  const fragment = resultCardTemplate.content.cloneNode(true);
  const block = fragment.querySelector('.result-block');
  const label = fragment.querySelector('.label');
  const row = fragment.querySelector('.card-row');

  block.classList.toggle('result-block-wide', isWide);
  label.textContent = labelText;
  renderCardRow(row, cardsText, '-');

  return fragment;
}

function createTextBlock(labelText, value, isWide = false) {
  const fragment = resultTextTemplate.content.cloneNode(true);
  const block = fragment.querySelector('.result-block');
  const label = fragment.querySelector('.label');
  const strong = fragment.querySelector('strong');

  block.classList.toggle('result-block-wide', isWide);
  label.textContent = labelText;
  strong.textContent = value ?? '-';

  return fragment;
}

function createRawBlock(labelText, value) {
  const fragment = resultRawTemplate.content.cloneNode(true);
  const block = fragment.querySelector('.result-block');
  const label = fragment.querySelector('.label');
  const pre = fragment.querySelector('pre');

  label.textContent = labelText;
  pre.textContent = value ?? 'Resultado aparecerá aqui.';
  block.classList.add('result-block-wide');

  return fragment;
}

function renderError(message) {
  handResult.innerHTML = '';
  handResult.classList.add('is-error');
  handResult.appendChild(createCardBlock('Cartas', ''));
  handResult.appendChild(createTextBlock('Ranking', '-'));
  handResult.appendChild(createTextBlock('Descrição', message, true));
  handResult.appendChild(createRawBlock('Resposta da API', message));
}

function renderSuccess(payload) {
  handResult.innerHTML = '';
  handResult.classList.remove('is-error');
  handResult.appendChild(createCardBlock('Cartas', payload.cards ?? ''));
  handResult.appendChild(createTextBlock('Ranking', payload.ranking ?? '-'));
  handResult.appendChild(createTextBlock('Descrição', payload.description ?? '-', true));
  handResult.appendChild(createRawBlock('Resposta da API', JSON.stringify(payload, null, 2)));
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
      renderError(payload.error ?? 'Falha ao avaliar a mão.');
      return;
    }

    renderSuccess(payload);
  });
}
