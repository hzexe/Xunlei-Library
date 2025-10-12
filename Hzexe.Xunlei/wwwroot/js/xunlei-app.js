// 迅雷远程下载应用JavaScript

// 全局变量
window.currentUser = null;
let tasks = {
    downloading: [],
    completed: []
};
let connection = null; // 修改为与initSignalR函数中使用的变量名
let qrCodeCheckInterval = null;

// DOM加载完成后初始化应用
document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
});

// 初始化应用
function initializeApp() {
    initEventListeners();
    checkLoginStatus();
    loadTasks();
    initSignalR();
}

// 初始化事件监听器
function initEventListeners() {
    // 侧边栏导航切换
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const tabName = this.getAttribute('data-tab');
            switchTab(tabName);
        });
    });

    // 新建任务按钮
    document.getElementById('new-task-btn').addEventListener('click', function() {
        const modal = new bootstrap.Modal(document.getElementById('newTaskModal'));
        modal.show();
    });

    // 登录按钮
    document.getElementById('login-btn').addEventListener('click', showLoginModal);

    // 密码登录按钮
    document.getElementById('passwordLoginBtn').addEventListener('click', function() {
        passwordLogin();
    });

    // 创建任务按钮
    document.getElementById('createTaskBtn').addEventListener('click', function() {
        createDownloadTask();
    });

    // 扫码登录标签切换
    document.getElementById('qrcode-tab').addEventListener('shown.bs.tab', function() {
        loadQrCode();
    });
    
    // 密码登录标签切换 - 停止二维码检查
    document.getElementById('password-tab').addEventListener('shown.bs.tab', function() {
        if (qrCodeCheckInterval) {
            clearInterval(qrCodeCheckInterval);
            qrCodeCheckInterval = null;
        }
    });
}

// 切换标签页
function switchTab(tabName) {
    // 更新导航栏激活状态
    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });
    document.querySelector(`[data-tab="${tabName}"]`).classList.add('active');
    
    // 同时更新nav-item的active状态
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active');
    });
    document.querySelector(`[data-tab="${tabName}"]`).parentElement.classList.add('active');

    // 更新badge样式 - 确保激活标签的badge使用primary样式，非激活标签的badge使用secondary样式
    document.getElementById('downloading-count').className = tabName === 'downloading' ? 'badge bg-primary' : 'badge bg-secondary';
    document.getElementById('completed-count').className = tabName === 'completed' ? 'badge bg-primary' : 'badge bg-secondary';

    // 显示对应内容 - 修复ID不一致问题
    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.remove('active');
    });
    document.getElementById(`${tabName}-content`).classList.add('active');

    // 加载对应任务
    loadTasks(tabName);
}

// 处理API错误
function handleApiError(error, defaultMessage) {
    console.error('API Error:', error);
    
    // 尝试解析错误响应
    let errorMessage = defaultMessage;
    if (error.response) {
        try {
            const errorData = error.response.data;
            
            // 检查标准错误格式
            if (errorData.detail) {
                errorMessage = errorData.detail;
            } 
            // 检查message字段
            else if (errorData.message) {
                errorMessage = errorData.message;
            }
            // 检查errors对象
            else if (errorData.errors) {
                const errors = Object.values(errorData.errors).flat();
                if (errors.length > 0) {
                    errorMessage = errors.join(', ');
                }
            }
        } catch (e) {
            console.error('Failed to parse error response:', e);
        }
    }
    
    // 显示错误提示
    showToast(errorMessage, 'danger');
    return errorMessage;
}

// 检查登录状态
async function checkLoginStatus() {
    try {
        const response = await fetch('/api/auth/info');
        if (response.ok) {
            const data = await response.json();
            // 修改为匹配后端实际返回的格式
            if (data.isLoggedIn && data.username) {
                window.currentUser = { username: data.username };
                updateUserStatus();
                return true;
            }
        }
        // 如果API调用失败或返回未登录状态，确保UI显示未登录状态
        window.currentUser = null;
        updateUserStatus();
        return false;
    } catch (error) {
        console.error('检查登录状态失败:', error);
        // 如果发生错误，确保UI显示未登录状态
        window.currentUser = null;
        updateUserStatus();
        return false;
    }
}

// 更新用户状态显示
function updateUserStatus() {
    const usernameDisplay = document.getElementById('username-display');
    const loginBtn = document.getElementById('login-btn');
    const userAction = document.querySelector('.user-action');
    
    if (window.currentUser) {
        usernameDisplay.textContent = window.currentUser.username || '已登录';
        
        // 创建下拉菜单结构
        userAction.innerHTML = `
            <div class="dropdown">
                <a href="#" class="dropdown-toggle" id="user-dropdown" data-bs-toggle="dropdown" aria-expanded="false">
                    ${window.currentUser.username || '已登录'}
                </a>
                <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="user-dropdown">
                    <li><a class="dropdown-item" href="#" id="logout-btn">
                        <i class="bi bi-box-arrow-right"></i> 退出登录
                    </a></li>
                </ul>
            </div>
        `;
        
        // 添加退出登录事件监听器
        document.getElementById('logout-btn').addEventListener('click', logout);
    } else {
        usernameDisplay.textContent = '未登录';
        
        // 恢复为简单的登录按钮
        userAction.innerHTML = `<a href="#" id="login-btn">登录</a>`;
        
        // 添加登录模态框事件监听器
        document.getElementById('login-btn').addEventListener('click', function(e) {
            e.preventDefault();
            showLoginModal();
        });
    }
}

// 显示登录模态框
function showLoginModal() {
    // 确保之前的模态框实例被销毁
    const modalElement = document.getElementById('loginModal');
    const modalInstance = bootstrap.Modal.getInstance(modalElement);
    if (modalInstance) {
        modalInstance.dispose();
    }
    
    // 移除可能存在的旧蒙版
    const existingBackdrop = document.querySelector('.modal-backdrop');
    if (existingBackdrop) {
        existingBackdrop.remove();
    }
    
    // 确保body没有modal-open类
    document.body.classList.remove('modal-open');
    document.body.style.overflow = '';
    
    // 确保密码登录标签页是激活状态
    const passwordTab = document.getElementById('password-tab');
    const passwordPane = document.getElementById('password-login');
    const qrcodeTab = document.getElementById('qrcode-tab');
    const qrcodePane = document.getElementById('qrcode-login');
    
    // 重置标签页状态
    passwordTab.classList.add('active');
    passwordTab.setAttribute('aria-selected', 'true');
    passwordPane.classList.add('show', 'active');
    
    qrcodeTab.classList.remove('active');
    qrcodeTab.setAttribute('aria-selected', 'false');
    qrcodePane.classList.remove('show', 'active');
    
    // 停止二维码检查
    if (qrCodeCheckInterval) {
        clearInterval(qrCodeCheckInterval);
        qrCodeCheckInterval = null;
    }
    
    // 创建新的模态框实例并显示
    const modal = new bootstrap.Modal(modalElement, {
        backdrop: 'static',
        keyboard: false
    });
    modal.show();
    
    // 确保模态框完全显示后调整位置
    modalElement.addEventListener('shown.bs.modal', function () {
        // 强制重绘以确保模态框正确显示
        modalElement.style.display = 'block';
        modalElement.style.zIndex = '1057';
        
        // 确保蒙版层正确显示
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) {
            backdrop.style.zIndex = '1054';
        }
        
        document.body.classList.add('modal-open');
        
        // 确保表单元素可见
        const formElements = passwordPane.querySelectorAll('.form-control, .form-label, .btn');
        formElements.forEach(el => {
            el.style.display = 'block';
            el.style.visibility = 'visible';
            el.style.opacity = '1';
        });
    }, { once: true });
}

// 密码登录
async function passwordLogin() {
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    
    if (!username || !password) {
        showToast('请输入用户名和密码', 'warning');
        return;
    }
    
    try {
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ username, password })
        });
        
        const data = await response.json();
        
        if (response.ok && data.success) {
            window.currentUser = { username: data.username };
            updateUserStatus();
            bootstrap.Modal.getInstance(document.getElementById('loginModal')).hide();
            showToast('登录成功', 'success');
        } else {
            handleApiError({ response }, '登录失败，请检查用户名和密码');
        }
    } catch (error) {
        handleApiError(error, '登录失败，请检查网络连接');
    }
}

// 退出登录
async function logout() {
    try {
        const response = await fetch('/api/auth/logout', {
            method: 'POST'
        });
        
        if (response.ok) {
            window.currentUser = null;
            updateUserStatus();
            showToast('退出登录成功', 'success');
        } else {
            handleApiError({ response }, '退出登录失败');
        }
    } catch (error) {
        handleApiError(error, '退出登录失败，请检查网络连接');
    }
}

// 加载二维码
async function loadQrCode() {
    const qrCodeContainer = document.getElementById('qrcode-container');
    qrCodeContainer.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">加载中...</span></div>';
    
    try {
        const response = await fetch('/api/auth/qrcode');
        if (response.ok) {
            const data = await response.json();
            if (data.success && data.data && data.data.qrCodeUrl) {
                qrCodeContainer.innerHTML = `<img src="${data.data.qrCodeUrl}" alt="登录二维码" class="img-fluid">`;
                startQrCodeCheck(data.data.qrCodeId);
            } else {
                handleApiError({ response }, '获取二维码失败');
            }
        } else {
            handleApiError({ response }, '获取二维码失败');
        }
    } catch (error) {
        handleApiError(error, '获取二维码失败，请检查网络连接');
    }
}

// 开始检查二维码状态
function startQrCodeCheck(qrCodeId) {
    if (qrCodeCheckInterval) {
        clearInterval(qrCodeCheckInterval);
    }
    
    // 获取登录模态框实例
    const loginModal = document.getElementById('loginModal');
    const modalInstance = bootstrap.Modal.getInstance(loginModal);
    
    // 监听模态框关闭事件，停止二维码检查
    const handleModalHidden = () => {
        if (qrCodeCheckInterval) {
            clearInterval(qrCodeCheckInterval);
            qrCodeCheckInterval = null;
        }
        loginModal.removeEventListener('hidden.bs.modal', handleModalHidden);
    };
    loginModal.addEventListener('hidden.bs.modal', handleModalHidden);
    
    qrCodeCheckInterval = setInterval(async () => {
        try {
            // 检查是否切换到了密码登录标签页
            const passwordTab = document.getElementById('password-tab');
            if (passwordTab && passwordTab.classList.contains('active')) {
                clearInterval(qrCodeCheckInterval);
                qrCodeCheckInterval = null;
                return;
            }
            
            const response = await fetch(`/api/auth/qrcode/${qrCodeId}/status`);
            if (response.ok) {
                const data = await response.json();
                if (data.success && data.data) {
                    const status = data.data.status;
                    
                    if (status === 'waiting') {
                        // 继续等待
                    } else if (status === 'confirmed') {
                        clearInterval(qrCodeCheckInterval);
                        qrCodeCheckInterval = null;
                        window.currentUser = data.data.user;
                        updateUserStatus();
                        modalInstance.hide();
                        showToast('登录成功', 'success');
                    }
                }
            }
        } catch (error) {
            console.error('检查二维码状态失败:', error);
        }
    }, 2000);
}

// 加载任务列表
async function loadTasks(tabName = null) {
    if (!tabName) {
        // 获取当前激活的标签页
        const activeTab = document.querySelector('.nav-item.active .nav-link');
        if (activeTab) {
            tabName = activeTab.getAttribute('data-tab');
        } else {
            tabName = 'downloading';
        }
    }
    
    const loadingIndicator = document.getElementById(`${tabName}-loading`);
    const tasksContainer = document.getElementById(`${tabName}-tasks-container`);
    
    // 显示加载指示器
    loadingIndicator.classList.remove('d-none');
    tasksContainer.innerHTML = '';
    
    try {
        // 修改为匹配后端实际的API端点
        let endpoint;
        if (tabName === 'downloading') {
            endpoint = '/api/tasks/downloading'; // 使用后端实际提供的端点
        } else {
            endpoint = '/api/tasks/completed'; // 使用后端实际提供的端点
        }
        
        const response = await fetch(endpoint);
        if (response.ok) {
            const data = await response.json();
            // 修改为匹配后端实际返回的格式
            if (data && data.success) {
                // 确保数据正确赋值到对应的任务列表
                if (tabName === 'downloading') {
                    tasks.downloading = data.tasks || [];
                } else if (tabName === 'completed') {
                    tasks.completed = data.tasks || [];
                }
                
                // 渲染当前标签页的任务
                renderTasks(tabName);
                updateTaskCounts();
            } else {
                // 如果API返回成功但没有success字段，尝试直接使用tasks字段
                if (data && data.tasks) {
                    if (tabName === 'downloading') {
                        tasks.downloading = data.tasks || [];
                    } else if (tabName === 'completed') {
                        tasks.completed = data.tasks || [];
                    }
                    
                    // 渲染当前标签页的任务
                    renderTasks(tabName);
                    updateTaskCounts();
                } else {
                    // 如果没有任务，显示空状态
                    if (tabName === 'downloading') {
                        tasks.downloading = [];
                    } else if (tabName === 'completed') {
                        tasks.completed = [];
                    }
                    renderTasks(tabName);
                    updateTaskCounts();
                }
            }
        } else {
            handleApiError({ response }, '加载任务列表失败');
        }
    } catch (error) {
        console.error('加载任务失败:', error);
        // 显示错误信息，但不阻止应用继续运行
        tasksContainer.innerHTML = `
            <div class="alert alert-warning">
                <i class="bi bi-exclamation-triangle"></i>
                加载任务列表失败，请检查网络连接
            </div>
        `;
    } finally {
        // 隐藏加载指示器
        loadingIndicator.classList.add('d-none');
    }
}

// 渲染任务列表
function renderTasks(tabName) {
    const tasksContainer = document.getElementById(`${tabName}-tasks-container`);
    if (!tasksContainer) return;
    
    // 清空容器
    tasksContainer.innerHTML = '';
    
    const tasksList = tabName === 'downloading' ? tasks.downloading : tasks.completed;
    
    // 创建表格
    const table = document.createElement('table');
    table.className = 'table table-striped table-hover';
    
    // 创建表头
    const thead = document.createElement('thead');
    const headerRow = document.createElement('tr');
    
    if (tabName === 'downloading') {
        headerRow.innerHTML = `
            <th>文件名</th>
            <th>大小</th>
            <th>进度</th>
            <th>速度</th>
        `;
    } else {
        headerRow.innerHTML = `
            <th>文件名</th>
            <th>大小</th>
            <th>完成时间</th>
        `;
    }
    
    thead.appendChild(headerRow);
    table.appendChild(thead);
    
    // 创建表体
    const tbody = document.createElement('tbody');
    tbody.id = `${tabName}-tasks`;
    
    if (tasksList.length === 0) {
        const emptyRow = document.createElement('tr');
        emptyRow.innerHTML = `<td colspan="${tabName === 'downloading' ? '4' : '3'}" class="text-center">暂无任务</td>`;
        tbody.appendChild(emptyRow);
    } else {
        tasksList.forEach(task => {
            const row = document.createElement('tr');
            
            // 格式化文件大小
            const formatFileSize = (bytes) => {
                if (bytes === 0) return '0 B';
                const k = 1024;
                const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
                const i = Math.floor(Math.log(bytes) / Math.log(k));
                return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
            };
            
            // 修改为使用后端实际返回的字段名
            const totalSize = task.resourceSize || 0;
            const downloadedSize = task.totalReceiveSize || 0;
            
            // 使用后端推送的进度值，如果没有则计算
            const progress = task.progress !== undefined ? task.progress : (totalSize > 0 ? Math.round((downloadedSize / totalSize) * 100) : 0);
            
            // 获取文件名，优先使用name字段，其次使用fileName，最后使用URL
            const getFileName = (task) => {
                if (task.name && task.name.trim() !== '') {
                    return task.name;
                }
                if (task.fileName && task.fileName.trim() !== '') {
                    return task.fileName;
                }
                if (task.url) {
                    // 从URL中提取文件名
                    const url = new URL(task.url);
                    const pathname = url.pathname;
                    const filename = pathname.substring(pathname.lastIndexOf('/') + 1);
                    return filename || '未知文件';
                }
                return '未知文件';
            };
            
            const fileName = getFileName(task);
            
            // 为行元素添加任务ID作为数据属性，便于后续更新
            row.setAttribute('data-task-id', task.id || task.taskId);
            
            if (tabName === 'downloading') {
                // 下载中任务表格 - 移除操作列
                row.innerHTML = `
                    <td>${fileName}</td>
                    <td>${formatFileSize(totalSize)}</td>
                    <td>
                        <div class="progress">
                            <div class="progress-bar" role="progressbar" style="width: ${progress}%">
                                ${progress}%
                            </div>
                        </div>
                    </td>
                    <td>${task.speed ? formatFileSize(task.speed) + '/s' : '0 B/s'}</td>
                `;
            } else {
                // 已完成任务表格 - 移除删除按钮
                const completionTime = task.completionTime ? 
                    new Date(task.completionTime).toLocaleString() : 
                    '未知';
                    
                row.innerHTML = `
                    <td>${fileName}</td>
                    <td>${formatFileSize(totalSize)}</td>
                    <td>${completionTime}</td>
                `;
            }
            
            tbody.appendChild(row);
        });
    }
    
    table.appendChild(tbody);
    tasksContainer.appendChild(table);
}

// 切换标签页
function switchTab(tabName) {
    // 更新导航栏激活状态
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active');
    });
    document.querySelector(`[data-tab="${tabName}"]`).parentElement.classList.add('active');

    // 显示对应内容 - 修复ID不一致问题
    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.remove('active');
    });
    document.getElementById(`${tabName}-content`).classList.add('active');

    // 加载对应任务
    loadTasks(tabName);
}

// 处理任务操作
async function handleTaskAction(taskId, action) {
    try {
        let endpoint;
        let method;
        
        switch (action) {
            case 'pause':
                endpoint = `/api/tasks/${taskId}/pause`;
                method = 'POST';
                break;
            case 'resume':
                endpoint = `/api/tasks/${taskId}/resume`;
                method = 'POST';
                break;
            case 'delete':
                if (!confirm('确定要删除此任务吗？')) {
                    return;
                }
                endpoint = `/api/tasks/${taskId}`;
                method = 'DELETE';
                break;
            default:
                return;
        }
        
        const response = await fetch(endpoint, { method });
        
        if (response.ok) {
            const data = await response.json();
            if (data.success) {
                showToast(`任务${action === 'pause' ? '已暂停' : action === 'resume' ? '已恢复' : '已删除'}`, 'success');
                loadTasks();
            } else {
                handleApiError({ response }, `任务${action}失败`);
            }
        } else {
            handleApiError({ response }, `任务${action}失败`);
        }
    } catch (error) {
        handleApiError(error, `任务${action}失败，请检查网络连接`);
    }
}

// 创建分页
function createPagination(tabName) {
    const paginationContainer = document.getElementById(`${tabName}-pagination`);
    paginationContainer.innerHTML = '';
    
    // 这里简化处理，实际应用中应该根据总任务数和每页显示数量来计算分页
    // 当前仅作为示例
}

// 更新任务计数
async function updateTaskCounts() {
    try {
        // 下载中任务计数 - 使用后端实际提供的端点
        try {
            const downloadingResponse = await fetch('/api/tasks/downloading/count');
            if (downloadingResponse.ok) {
                const downloadingData = await downloadingResponse.json();
                // 修改为匹配后端实际返回的格式
                if (downloadingData && downloadingData.success) {
                    document.getElementById('downloading-count').textContent = downloadingData.count || 0;
                } else {
                    document.getElementById('downloading-count').textContent = 0;
                }
            } else {
                document.getElementById('downloading-count').textContent = tasks.downloading.length;
            }
        } catch (error) {
            console.error('获取下载中任务计数失败:', error);
            document.getElementById('downloading-count').textContent = tasks.downloading.length;
        }
        
        // 已完成任务计数 - 使用后端实际提供的端点
        try {
            const completedResponse = await fetch('/api/tasks/completed/count');
            if (completedResponse.ok) {
                const completedData = await completedResponse.json();
                // 修改为匹配后端实际返回的格式
                if (completedData && completedData.success) {
                    document.getElementById('completed-count').textContent = completedData.count || 0;
                } else {
                    document.getElementById('completed-count').textContent = 0;
                }
            } else {
                document.getElementById('completed-count').textContent = tasks.completed.length;
            }
        } catch (error) {
            console.error('获取已完成任务计数失败:', error);
            document.getElementById('completed-count').textContent = tasks.completed.length;
        }
    } catch (error) {
        console.error('更新任务计数失败:', error);
    }
}

// 创建下载任务
async function createDownloadTask() {
    const urlInput = document.getElementById('url');
    const url = urlInput.value.trim();
    
    if (!url) {
        showToast('请输入下载链接', 'warning');
        return;
    }
    
    // 验证URL协议
    const supportedProtocols = ['http:', 'https:', 'ftp:', 'ed2k:', 'magnet:'];
    let isValidProtocol = false;
    
    try {
        const urlObj = new URL(url);
        if (supportedProtocols.includes(urlObj.protocol)) {
            isValidProtocol = true;
        }
    } catch (e) {
        // 如果URL解析失败，可能是magnet链接或其他特殊格式
        if (url.startsWith('magnet:')) {
            isValidProtocol = true;
        }
    }
    
    if (!isValidProtocol) {
        showToast('不支持的协议，请使用HTTP、HTTPS、FTP、ED2K或Magnet链接', 'danger');
        return;
    }
    
    try {
        const response = await fetch('/api/tasks/download', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ url })
        });
        
        const data = await response.json();
        
        if (response.ok && data.success) {
            urlInput.value = '';
            bootstrap.Modal.getInstance(document.getElementById('newTaskModal')).hide();
            showToast('任务创建成功', 'success');
            loadTasks();
        } else {
            handleApiError({ response }, '创建任务失败');
        }
    } catch (error) {
        handleApiError(error, '创建任务失败，请检查网络连接');
    }
}

// 初始化SignalR连接
function initSignalR() {
    // 创建与后端的SignalR连接
    connection = new signalR.HubConnectionBuilder()
        .withUrl('/taskUpdateHub') // 修改为与服务器配置一致的路径
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // 启动连接
    async function startConnection() {
        try {
            await connection.start();
            console.log('SignalR连接已建立');
            showToast('实时连接已建立', 'success');
        } catch (err) {
            console.error('SignalR连接失败:', err);
            showToast('实时连接建立失败，将使用轮询方式更新', 'warning');
            // 如果连接失败，设置定期刷新任务列表
            setInterval(loadTasks, 5000); // 每5秒刷新一次
        }
    }

    // 监听连接关闭事件
    connection.onclose(async () => {
        await startConnection();
    });

    // 监听任务更新事件
    connection.on('updatetask', (updatedTask) => {
        console.log('收到任务更新:', updatedTask);
        updateTaskInList(updatedTask);
        updateTaskCounts();
    });

    // 监听任务添加事件
    connection.on('TaskAdded', (newTask) => {
        console.log('收到新任务:', newTask);
        loadTasks(); // 重新加载任务列表
        updateTaskCounts();
    });

    // 监听任务删除事件
    connection.on('TaskDeleted', (taskId) => {
        console.log('任务已删除:', taskId);
        loadTasks(); // 重新加载任务列表
        updateTaskCounts();
    });

    // 启动连接
    startConnection();
}

// 更新任务列表中的任务
function updateTaskInList(updatedTask) {
    // 查找并更新下载中任务
    const downloadingIndex = tasks.downloading.findIndex(task => task.id === updatedTask.taskId);
    if (downloadingIndex !== -1) {
        // 合并更新数据，保留原有数据中未更新的字段
        const existingTask = tasks.downloading[downloadingIndex];
        const mergedTask = { ...existingTask, ...updatedTask };
        
        // 确保ID字段正确
        if (!mergedTask.id && mergedTask.taskId) {
            mergedTask.id = mergedTask.taskId;
        }
        
        tasks.downloading[downloadingIndex] = mergedTask;
        
        // 如果任务已完成，移动到已完成列表
        if (mergedTask.status === 'completed' || mergedTask.status === 5) {
            tasks.downloading.splice(downloadingIndex, 1);
            // 检查是否已在已完成列表中，避免重复添加
            const completedIndex = tasks.completed.findIndex(task => task.id === updatedTask.taskId);
            if (completedIndex === -1) {
                tasks.completed.unshift(mergedTask);
            } else {
                // 如果已存在，则更新
                tasks.completed[completedIndex] = mergedTask;
            }
        }
        
        // 重新渲染当前标签页
        const activeTab = document.querySelector('.nav-item.active .nav-link');
        if (activeTab) {
            const tabName = activeTab.getAttribute('data-tab');
            if (tabName === 'downloading' || (tabName === 'completed' && (mergedTask.status === 'completed' || mergedTask.status === 5))) {
                renderTasks(tabName);
            }
        }
        
        updateTaskCounts();
        return;
    }
    
    // 查找并更新已完成任务
    const completedIndex = tasks.completed.findIndex(task => task.id === updatedTask.taskId);
    if (completedIndex !== -1) {
        // 合并更新数据，保留原有数据中未更新的字段
        const existingTask = tasks.completed[completedIndex];
        const mergedTask = { ...existingTask, ...updatedTask };
        
        // 确保ID字段正确
        if (!mergedTask.id && mergedTask.taskId) {
            mergedTask.id = mergedTask.taskId;
        }
        
        tasks.completed[completedIndex] = mergedTask;
        
        // 重新渲染已完成标签页
        const activeTab = document.querySelector('.nav-item.active .nav-link');
        if (activeTab && activeTab.getAttribute('data-tab') === 'completed') {
            renderTasks('completed');
        }
    } else {
        // 如果任务在两个列表中都不存在，根据状态添加到相应列表
        if (updatedTask.status !== 'completed' && updatedTask.status !== 5) {
            // 检查是否已在下载中列表中，避免重复添加
            const existsInDownloading = tasks.downloading.some(task => task.id === updatedTask.taskId);
            if (!existsInDownloading) {
                // 确保ID字段正确
                const newTask = { ...updatedTask };
                if (!newTask.id && newTask.taskId) {
                    newTask.id = newTask.taskId;
                }
                
                tasks.downloading.push(newTask);
                
                // 重新渲染下载中标签页
                const activeTab = document.querySelector('.nav-item.active .nav-link');
                if (activeTab && activeTab.getAttribute('data-tab') === 'downloading') {
                    renderTasks('downloading');
                }
            }
        } else {
            // 检查是否已在已完成列表中，避免重复添加
            const existsInCompleted = tasks.completed.some(task => task.id === updatedTask.taskId);
            if (!existsInCompleted) {
                // 确保ID字段正确
                const newTask = { ...updatedTask };
                if (!newTask.id && newTask.taskId) {
                    newTask.id = newTask.taskId;
                }
                
                tasks.completed.unshift(newTask);
                
                // 重新渲染已完成标签页
                const activeTab = document.querySelector('.nav-item.active .nav-link');
                if (activeTab && activeTab.getAttribute('data-tab') === 'completed') {
                    renderTasks('completed');
                }
            }
        }
        
        updateTaskCounts();
    }
}

// 从任务列表中移除任务
function removeTaskFromList(taskId) {
    // 从下载中任务列表移除
    const downloadingIndex = tasks.downloading.findIndex(task => task.id === taskId);
    if (downloadingIndex !== -1) {
        tasks.downloading.splice(downloadingIndex, 1);
        
        // 重新渲染下载中标签页
        const activeTab = document.querySelector('.nav-item.active .nav-link');
        if (activeTab && activeTab.getAttribute('data-tab') === 'downloading') {
            renderTasks('downloading');
        }
        
        updateTaskCounts();
        return;
    }
    
    // 从已完成任务列表移除
    const completedIndex = tasks.completed.findIndex(task => task.id === taskId);
    if (completedIndex !== -1) {
        tasks.completed.splice(completedIndex, 1);
        
        // 重新渲染已完成标签页
        const activeTab = document.querySelector('.nav-item.active .nav-link');
        if (activeTab && activeTab.getAttribute('data-tab') === 'completed') {
            renderTasks('completed');
        }
        
        updateTaskCounts();
    }
}

// 显示提示消息
function showToast(message, type = 'info') {
    // 创建toast容器（如果不存在）
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }
    
    // 创建toast元素
    const toastId = 'toast-' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header bg-${type} text-white">
                <strong class="me-auto">提示</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;
    
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    
    // 显示toast
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement);
    toast.show();
    
    // 自动移除
    toastElement.addEventListener('hidden.bs.toast', function() {
        toastElement.remove();
    });
}