// Конфигурация API
const API_BASE_URL = '/api'; // Прокси через nginx к Gateway

// Утилиты
function getToken() {
    return localStorage.getItem('token');
}

function setToken(token) {
    localStorage.setItem('token', token);
}

function removeToken() {
    localStorage.removeItem('token');
}

function showError(elementId, message) {
    const element = document.getElementById(elementId);
    element.textContent = message;
    element.classList.add('show');
    setTimeout(() => element.classList.remove('show'), 5000);
}

// Работа с API
async function apiCall(endpoint, options = {}) {
    const token = getToken();
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };
    
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...options,
            headers
        });
        
        const data = await response.json();
        
        if (!response.ok && response.status !== 400) {
            throw new Error(data.errorMessage || 'Ошибка запроса');
        }
        
        return { ...data, httpStatus: response.status };
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}

// Авторизация
async function handleLogin(event) {
    event.preventDefault();
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;
    
    try {
        const result = await apiCall('/Authorization/Authenticate', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });
        
        if (result.statusCode === 200 && result.token) {
            setToken(result.token);
            showMainPage();
        } else {
            showError('loginError', result.errorMessage || 'Ошибка входа');
        }
    } catch (error) {
        showError('loginError', error.message || 'Ошибка соединения');
    }
}

async function handleRegister(event) {
    event.preventDefault();
    const email = document.getElementById('registerEmail').value;
    const password = document.getElementById('registerPassword').value;
    
    try {
        const result = await apiCall('/Authorization/Register', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });
        
        if (result.statusCode === 200) {
            document.getElementById('registerSuccess').style.display = 'block';
            document.getElementById('registerUserId').textContent = result.userID || 'UserID не получен';
            document.getElementById('registerForm').querySelector('form').reset();
        } else {
            showError('registerError', result.errorMessage || 'Ошибка регистрации');
        }
    } catch (error) {
        showError('registerError', error.message || 'Ошибка соединения');
    }
}

function handleLogout() {
    removeToken();
    showAuthPage();
}

// Навигация по страницам
function showAuthPage() {
    document.getElementById('authPage').style.display = 'block';
    document.getElementById('mainPage').style.display = 'none';
}

function showMainPage() {
    document.getElementById('authPage').style.display = 'none';
    document.getElementById('mainPage').style.display = 'block';
    loadExpenses();
}

function showAuthTab(tab) {
    if (tab === 'login') {
        document.getElementById('loginForm').style.display = 'block';
        document.getElementById('registerForm').style.display = 'none';
        document.querySelectorAll('.auth-tabs .tab-btn')[0].classList.add('active');
        document.querySelectorAll('.auth-tabs .tab-btn')[1].classList.remove('active');
    } else {
        document.getElementById('loginForm').style.display = 'none';
        document.getElementById('registerForm').style.display = 'block';
        document.querySelectorAll('.auth-tabs .tab-btn')[0].classList.remove('active');
        document.querySelectorAll('.auth-tabs .tab-btn')[1].classList.add('active');
    }
}

function showTab(tab) {
    if (tab === 'expenses') {
        document.getElementById('expensesTab').style.display = 'block';
        document.getElementById('historyTab').style.display = 'none';
        document.querySelectorAll('.tabs .tab-btn')[0].classList.add('active');
        document.querySelectorAll('.tabs .tab-btn')[1].classList.remove('active');
    } else {
        document.getElementById('expensesTab').style.display = 'none';
        document.getElementById('historyTab').style.display = 'block';
        document.querySelectorAll('.tabs .tab-btn')[0].classList.remove('active');
        document.querySelectorAll('.tabs .tab-btn')[1].classList.add('active');
    }
}

// Запланированные расходы
async function loadExpenses() {
    const listElement = document.getElementById('expensesList');
    listElement.innerHTML = '<div class="loading">Загрузка...</div>';
    
    try {
        const result = await apiCall('/ExpensesScheduler/GetScheduledExpenses');
        
        if (result.statusCode === 200 && result.scheduledExpenses) {
            if (result.scheduledExpenses.length === 0) {
                listElement.innerHTML = '<div class="info-message">Нет запланированных расходов</div>';
            } else {
                listElement.innerHTML = result.scheduledExpenses.map(expense => `
                    <div class="expense-item">
                        <div class="expense-item-info">
                            <div class="expense-item-title">${escapeHtml(expense.description)}</div>
                            <div class="expense-item-details">
                                ${getScheduleTypeText(expense)} | 
                                Создан: ${formatDate(expense.createdDate)}
                            </div>
                        </div>
                        <div class="expense-item-amount">${formatCurrency(expense.amount)}</div>
                        <div class="expense-item-actions">
                            <button class="btn btn-edit" onclick="editExpense('${expense.id}')">Редактировать</button>
                            <button class="btn btn-danger" onclick="deleteExpense('${expense.id}')">Удалить</button>
                        </div>
                    </div>
                `).join('');
            }
        } else {
            listElement.innerHTML = '<div class="error-message show">Ошибка загрузки расходов</div>';
        }
    } catch (error) {
        listElement.innerHTML = `<div class="error-message show">${error.message}</div>`;
    }
}

function getScheduleTypeText(expense) {
    if (expense.everyMonth) {
        return 'Ежемесячно';
    } else if (expense.happensInDays === 1) {
        return 'Ежедневно';
    } else if (expense.happensInDays === 7) {
        return 'Еженедельно';
    } else if (expense.happensInDays) {
        return `Каждые ${expense.happensInDays} дней`;
    } else {
        return 'Не указано';
    }
}

function showAddExpenseForm() {
    document.getElementById('expenseFormTitle').textContent = 'Добавить расход';
    document.getElementById('expenseForm').reset();
    document.getElementById('expenseId').value = '';
    // После сброса формы селект будет иметь значение "" (Произвольный период)
    // Нужно вызвать handleScheduleTypeChange чтобы показать поле "Через сколько дней"
    handleScheduleTypeChange();
    document.getElementById('expenseFormModal').style.display = 'block';
}

async function editExpense(id) {
    try {
        const result = await apiCall('/ExpensesScheduler/GetScheduledExpenses');
        if (result.statusCode === 200 && result.scheduledExpenses) {
            const expense = result.scheduledExpenses.find(e => e.id === id);
            if (expense) {
                document.getElementById('expenseFormTitle').textContent = 'Редактировать расход';
                document.getElementById('expenseId').value = expense.id;
                document.getElementById('expenseDescription').value = expense.description;
                document.getElementById('expenseAmount').value = expense.amount;
                document.getElementById('expenseOneTimeOnly').checked = expense.oneTimeOnly;
                
                if (expense.everyMonth) {
                    document.getElementById('expenseScheduleType').value = '2';
                } else if (expense.happensInDays === 1) {
                    document.getElementById('expenseScheduleType').value = '0';
                } else if (expense.happensInDays === 7) {
                    document.getElementById('expenseScheduleType').value = '1';
                } else {
                    document.getElementById('expenseScheduleType').value = '';
                    document.getElementById('expenseHappensInDays').value = expense.happensInDays || '';
                    document.getElementById('happensInDaysGroup').style.display = 'block';
                }
                
                handleScheduleTypeChange();
                document.getElementById('expenseFormModal').style.display = 'block';
            }
        }
    } catch (error) {
        showError('expenseFormError', error.message);
    }
}

function handleScheduleTypeChange() {
    const scheduleType = document.getElementById('expenseScheduleType').value;
    const happensInDaysGroup = document.getElementById('happensInDaysGroup');
    
    if (scheduleType === '') {
        happensInDaysGroup.style.display = 'block';
    } else {
        happensInDaysGroup.style.display = 'none';
        document.getElementById('expenseHappensInDays').value = '';
    }
}

async function handleSaveExpense(event) {
    event.preventDefault();
    
    const id = document.getElementById('expenseId').value;
    const description = document.getElementById('expenseDescription').value;
    const amount = parseFloat(document.getElementById('expenseAmount').value);
    const scheduleType = document.getElementById('expenseScheduleType').value;
    const happensInDays = document.getElementById('expenseHappensInDays').value;
    const oneTimeOnly = document.getElementById('expenseOneTimeOnly').checked;
    
    const requestBody = {
        description,
        amount,
        oneTimeOnly
    };
    
    if (scheduleType !== '') {
        requestBody.scheduleType = parseInt(scheduleType);
    } else if (happensInDays && happensInDays !== '') {
        requestBody.happensInDays = parseInt(happensInDays);
    }
    
    try {
        let result;
        if (id) {
            requestBody.expenseID = id;
            result = await apiCall('/ExpensesScheduler/UpdateScheduledExpense', {
                method: 'PUT',
                body: JSON.stringify(requestBody)
            });
        } else {
            result = await apiCall('/ExpensesScheduler/AddScheduledExpense', {
                method: 'POST',
                body: JSON.stringify(requestBody)
            });
        }
        
        if (result.statusCode === 200) {
            closeExpenseForm();
            loadExpenses();
        } else {
            showError('expenseFormError', result.errorMessage || 'Ошибка сохранения');
        }
    } catch (error) {
        showError('expenseFormError', error.message || 'Ошибка соединения');
    }
}

async function deleteExpense(id) {
    if (!confirm('Вы уверены, что хотите удалить этот расход?')) {
        return;
    }
    
    try {
        const result = await apiCall('/ExpensesScheduler/DeleteScheduledExpense', {
            method: 'DELETE',
            body: JSON.stringify({ expenseID: id })
        });
        
        if (result.statusCode === 200) {
            loadExpenses();
        } else {
            alert(result.errorMessage || 'Ошибка удаления');
        }
    } catch (error) {
        alert(error.message || 'Ошибка соединения');
    }
}

function closeExpenseForm() {
    document.getElementById('expenseFormModal').style.display = 'none';
    document.getElementById('expenseFormError').classList.remove('show');
}

// История расходов
async function loadHistory() {
    const startDate = document.getElementById('historyStartDate').value;
    const endDate = document.getElementById('historyEndDate').value;
    
    if (!startDate || !endDate) {
        alert('Выберите начальную и конечную дату');
        return;
    }
    
    const listElement = document.getElementById('historyList');
    listElement.innerHTML = '<div class="loading">Загрузка...</div>';
    
    try {
        const result = await apiCall(`/ExpensesScheduler/GetExpensesHistory?start=${startDate}&end=${endDate}`);
        
        if (result.statusCode === 200 && result.expensesHistory) {
            if (result.expensesHistory.length === 0) {
                listElement.innerHTML = '<div class="info-message">Нет расходов за выбранный период</div>';
            } else {
                listElement.innerHTML = result.expensesHistory.map(expense => `
                    <div class="history-item">
                        <div class="history-item-info">
                            <div class="history-item-title">${escapeHtml(expense.description)}</div>
                            <div class="history-item-details">${formatDate(expense.dateTime)}</div>
                        </div>
                        <div class="history-item-amount">${formatCurrency(expense.amount)}</div>
                    </div>
                `).join('');
            }
        } else {
            listElement.innerHTML = '<div class="error-message show">Ошибка загрузки истории</div>';
        }
    } catch (error) {
        listElement.innerHTML = `<div class="error-message show">${error.message}</div>`;
    }
}

// Утилиты форматирования
function formatCurrency(amount) {
    return new Intl.NumberFormat('ru-RU', {
        style: 'currency',
        currency: 'RUB'
    }).format(amount);
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('ru-RU', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Инициализация
document.addEventListener('DOMContentLoaded', () => {
    const token = getToken();
    if (token) {
        showMainPage();
    } else {
        showAuthPage();
    }
    
    // Установка дат по умолчанию для истории
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    document.getElementById('historyEndDate').value = today.toISOString().split('T')[0];
    document.getElementById('historyStartDate').value = firstDay.toISOString().split('T')[0];
});
