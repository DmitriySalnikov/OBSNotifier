namespace OBSNotifier
{
    namespace Tr
    {
        internal static class TrUtils
        {
            static string GetLocale()
            {
                return System.Globalization.CultureInfo.CurrentUICulture.Name;
            }

            public static string GetTranslation(ref Dictionary<string, string> dict)
            {
                if (dict.TryGetValue(GetLocale(), out string? val))
                {
                    return val;
                }
                return dict["en"];
            }
        }

        public static class TrResources
        {
            public static string LocalizationStatus
            {
                get => @"✔️  Fetching project info
Translated:
	- af: 0%
	- ar: 82%
	- ca: 0%
	- cs: 100%
	- da: 100%
	- de: 100%
	- el: 82%
	- es-ES: 89%
	- fi: 100%
	- fr: 89%
	- he: 0%
	- hu: 0%
	- it: 100%
	- ja: 81%
	- ko: 0%
	- nl: 89%
	- no: 88%
	- pl: 89%
	- pt-BR: 95%
	- pt-PT: 95%
	- ro: 100%
	- ru: 100%
	- sr: 0%
	- sv-SE: 100%
	- tr: 0%
	- vi: 0%
	- zh-CN: 93%
	- zh-TW: 0%
Approved:
	- af: 0%
	- ar: 0%
	- ca: 0%
	- cs: 0%
	- da: 0%
	- de: 0%
	- el: 0%
	- es-ES: 0%
	- fi: 0%
	- fr: 0%
	- he: 0%
	- hu: 0%
	- it: 0%
	- ja: 0%
	- ko: 0%
	- nl: 0%
	- no: 0%
	- pl: 0%
	- pt-BR: 0%
	- pt-PT: 0%
	- ro: 0%
	- ru: 100%
	- sr: 0%
	- sv-SE: 0%
	- tr: 0%
	- vi: 0%
	- zh-CN: 90%
	- zh-TW: 0%
";
            }
        }

        public static class MessageBox
        {
            /// <summary>
            /// "Error"
            /// </summary>
            public static string ErrorTitle { get => TrUtils.GetTranslation(ref errorTitle); }
            static Dictionary<string, string> errorTitle = new() { { "en", "Error" }, { "ru", "Ошибка" }, { "zh-CN", "错误" } };

        }

        public static class MessageBoxApp
        {
            /// <summary>
            /// "{0} is already running.\nThis application instance will be closed."
            /// </summary>
            public static string AlreadyRunning(string arg0) => string.Format(TrUtils.GetTranslation(ref alreadyRunning), arg0);
            static Dictionary<string, string> alreadyRunning = new() { { "en", "{0} is already running.\nThis application instance will be closed." }, { "ru", "{0} уже запущен.\nЭтот экземпляр приложения будет закрыт." }, { "zh-CN", "{0} 已经在运行。。\n此应用程序实例将被关闭。" } };

            /// <summary>
            /// "Application is already running"
            /// </summary>
            public static string AlreadyRunningTitle { get => TrUtils.GetTranslation(ref alreadyRunningTitle); }
            static Dictionary<string, string> alreadyRunningTitle = new() { { "en", "Application is already running" }, { "ru", "Приложение уже запущено" }, { "zh-CN", "应用程序已在运行" } };

            /// <summary>
            /// "Authentication failed"
            /// </summary>
            public static string AuthFailed { get => TrUtils.GetTranslation(ref authFailed); }
            static Dictionary<string, string> authFailed = new() { { "en", "Authentication failed" }, { "ru", "Ошибка авторизации" }, { "zh-CN", "鉴权失败/密码错误" } };

            /// <summary>
            /// "OBS kicked out this application"
            /// </summary>
            public static string KickedFailed { get => TrUtils.GetTranslation(ref kickedFailed); }
            static Dictionary<string, string> kickedFailed = new() { { "en", "OBS kicked out this application" } };

        }

        public static class MessageBoxVersionCheck
        {
            /// <summary>
            /// "Current version: {0}\nNew version: {1}\nWould you like to go to the download page?\n\nSelect 'No' to skip this version."
            /// </summary>
            public static string NewVersionAvailable(string arg0, string arg1) => string.Format(TrUtils.GetTranslation(ref newVersionAvailable), arg0, arg1);
            static Dictionary<string, string> newVersionAvailable = new() { { "en", "Current version: {0}\nNew version: {1}\nWould you like to go to the download page?\n\nSelect 'No' to skip this version." }, { "ru", "Текущая версия: {0}\nНовая версия: {1}\nПерейти на страницу загрузки?\n\nВыберите 'Нет' чтобы пропустить эту версию." }, { "zh-CN", "当前版本:{0} \n新版本 :{1}\n你想转到下载页面吗？\n选择“否”跳过此版本。" } };

            /// <summary>
            /// "A new version of {0} is available"
            /// </summary>
            public static string NewVersionAvailableTitle(string arg0) => string.Format(TrUtils.GetTranslation(ref newVersionAvailableTitle), arg0);
            static Dictionary<string, string> newVersionAvailableTitle = new() { { "en", "A new version of {0} is available" }, { "ru", "Доступна новая версия {0}" }, { "zh-CN", "{0} 的新版本可用" } };

            /// <summary>
            /// "You are using the latest version: {0}"
            /// </summary>
            public static string LatestVersion(string arg0) => string.Format(TrUtils.GetTranslation(ref latestVersion), arg0);
            static Dictionary<string, string> latestVersion = new() { { "en", "You are using the latest version: {0}" }, { "ru", "Вы используете последнюю версию: {0}" }, { "zh-CN", "您正在使用最新版本： {0}" } };

            /// <summary>
            /// "Failed to request info about the new version."
            /// </summary>
            public static string FailedRequest { get => TrUtils.GetTranslation(ref failedRequest); }
            static Dictionary<string, string> failedRequest = new() { { "en", "Failed to request info about the new version." }, { "ru", "Не удалось запросить информацию о новой версии." }, { "zh-CN", "获取新版本信息失败。" } };

            /// <summary>
            /// "Failed to get information about the new version."
            /// </summary>
            public static string FailedParseInfo { get => TrUtils.GetTranslation(ref failedParseInfo); }
            static Dictionary<string, string> failedParseInfo = new() { { "en", "Failed to get information about the new version." }, { "ru", "Не удалось получить информацию о новой версии." }, { "zh-CN", "无法获取关于新版本的信息。" } };

            /// <summary>
            /// "Failed to check for update."
            /// </summary>
            public static string FailedToCheck { get => TrUtils.GetTranslation(ref failedToCheck); }
            static Dictionary<string, string> failedToCheck = new() { { "en", "Failed to check for update." }, { "ru", "Не удалось проверить обновление." }, { "zh-CN", "检查更新失败。" } };

        }

        public static class MessageBoxAutostartScript
        {
            /// <summary>
            /// "Complete the configuration"
            /// </summary>
            public static string Title { get => TrUtils.GetTranslation(ref title); }
            static Dictionary<string, string> title = new() { { "en", "Complete the configuration" }, { "ru", "Завершите настройку" }, { "zh-CN", "完成配置" } };

            /// <summary>
            /// "The autostart script was successfully created!"
            /// </summary>
            public static string Created { get => TrUtils.GetTranslation(ref created); }
            static Dictionary<string, string> created = new() { { "en", "The autostart script was successfully created!" }, { "ru", "Скрипт автоматического запуска успешно создан!" }, { "zh-CN", "自动启动脚本创建成功！" } };

            /// <summary>
            /// "The autostart script has been successfully updated!"
            /// </summary>
            public static string Updated { get => TrUtils.GetTranslation(ref updated); }
            static Dictionary<string, string> updated = new() { { "en", "The autostart script has been successfully updated!" }, { "ru", "Скрипт автоматического запуска успешно обновлен!" }, { "zh-CN", "自动启动脚本更新成功！" } };

            /// <summary>
            /// "The path to the file has been copied to the clipboard."
            /// </summary>
            public static string PathCopied { get => TrUtils.GetTranslation(ref pathCopied); }
            static Dictionary<string, string> pathCopied = new() { { "en", "The path to the file has been copied to the clipboard." }, { "ru", "Путь к файлу скопирован в буфер обмена." }, { "zh-CN", "文件的路径已复制到剪贴板。" } };

            /// <summary>
            /// "If you have already added a script to OBS before, you can safely close this message.\nIf not, follow the instructions below."
            /// </summary>
            public static string AlreadyAdded { get => TrUtils.GetTranslation(ref alreadyAdded); }
            static Dictionary<string, string> alreadyAdded = new() { { "en", "If you have already added a script to OBS before, you can safely close this message.\nIf not, follow the instructions below." }, { "ru", "Если вы уже добавляли скрипт в OBS, вы можете спокойно закрыть это сообщение.\nЕсли нет, следуйте инструкциям ниже." }, { "zh-CN", "如果您之前已经向OBS添加了脚本，则可以安全地关闭此消息。\n\n如果没有，请按照以下说明进行操作。" } };

            /// <summary>
            /// "Now you need to add it to OBS."
            /// </summary>
            public static string NeedToAdd { get => TrUtils.GetTranslation(ref needToAdd); }
            static Dictionary<string, string> needToAdd = new() { { "en", "Now you need to add it to OBS." }, { "ru", "Теперь вам нужно добавить его в OBS." }, { "zh-CN", "现在你需要将它添加到OBS的脚本库。" } };

            /// <summary>
            /// "You need to open OBS and go to the script settings window.\nTools -> Scripts.\nThen click on the + button, paste the path from the clipboard in the 'File Name' field and click Open.\n\nSummary: Tools -> Scripts -> + button -> Paste the path in the 'File Name' field -> click Open.\n\nAfter that, {0} will automatically start with OBS."
            /// </summary>
            public static string Instruction(string arg0) => string.Format(TrUtils.GetTranslation(ref instruction), arg0);
            static Dictionary<string, string> instruction = new() { { "en", "You need to open OBS and go to the script settings window.\nTools -> Scripts.\nThen click on the + button, paste the path from the clipboard in the 'File Name' field and click Open.\n\nSummary: Tools -> Scripts -> + button -> Paste the path in the 'File Name' field -> click Open.\n\nAfter that, {0} will automatically start with OBS." }, { "ru", "Вам необходимо открыть OBS и перейти в окно настроек скриптов.\nСервис -> Скрипты.\nЗатем нажмите на кнопку +, вставьте путь из буфера обмена в поле 'Имя файла' и нажмите Открыть.\n\nВкратце: Сервис -> Скрипты -> Кнопка + -> Вставьте путь в поле 'Имя файла' -> Нажмите Открыть.\n\nПосле этого {0} будет запускаться автоматически вместе с OBS." }, { "zh-CN", "您需要打开OBS并转到脚本设置窗口。\n\n工具->脚本。\n\n然后单击+按钮，将剪贴板中的路径粘贴到“文件名”字段中然后确定并回到OBS脚本客户端检查是否已经出现脚本，如果已经添加进去那么操作就成功了。\n\n右面的选项是以低优先级启动请根据自己的情况进行启用\n\n之后，{0}工具将会伴随OBS启动。" } };

        }

        public static class AboutWindow
        {
            /// <summary>
            /// "This is a program for displaying notifications from OBS. In order for this program to work, you need to install OBS version 28+ with the built-in 'obs-websocket' version 5+."
            /// </summary>
            public static string Description { get => TrUtils.GetTranslation(ref description); }
            static Dictionary<string, string> description = new() { { "en", "This is a program for displaying notifications from OBS. In order for this program to work, you need to install OBS version 28+ with the built-in 'obs-websocket' version 5+." }, { "ru", "Это программа для отображения уведомлений из OBS. Для работы этой программы вам нужно установить OBS версии 28+ со встроенным 'obs-websocket' версии 5+." }, { "zh-CN", "这是一个显示 OBS 通知的程序。 为了使这个程序发挥作用，您需要安装OBS版本28+，并安装内置的 'obs-websocket' 版本 5+。" } };

            /// <summary>
            /// "View the source code"
            /// </summary>
            public static string ViewSourceCode { get => TrUtils.GetTranslation(ref viewSourceCode); }
            static Dictionary<string, string> viewSourceCode = new() { { "en", "View the source code" }, { "ru", "Просмотреть исходный код" }, { "zh-CN", "查看源代码" } };

        }

        public static class SettingsWindow
        {
            /// <summary>
            /// "In order for this application to work you need to connect to the obs-websocket plugin."
            /// </summary>
            public static string TopHint { get => TrUtils.GetTranslation(ref topHint); }
            static Dictionary<string, string> topHint = new() { { "en", "In order for this application to work you need to connect to the obs-websocket plugin." }, { "fr", "Pour que cette application fonctionne, vous devez vous connecter au plugin obs-websocket." }, { "ru", "Для работы этого приложения необходимо подключение к плагину obs-websocket." }, { "zh-CN", "为了使这个应用程序能够工作，您需要打开并连接到 obs-websocket 它就在工具的选项菜单里" } };

            /// <summary>
            /// "How do I enable 'obs-websocket'?"
            /// </summary>
            public static string TopHintHowTo { get => TrUtils.GetTranslation(ref topHintHowTo); }
            static Dictionary<string, string> topHintHowTo = new() { { "en", "How do I enable 'obs-websocket'?" }, { "fr", "Comment activer 'obs-websocket' ?" }, { "ru", "Как включить 'obs-websocket'?" }, { "zh-CN", "如何启用'obs-websocket'？" } };

            /// <summary>
            /// "1. Tools - 2. WebSocket Server Settings - 3. Enable WebSocket server\n?. Optional 'Enable Authentication'. To find out the current password, click 'Show Connect Info'.\nDon't forget to click Apply after making changes to the server."
            /// </summary>
            public static string TopHintTooltip { get => TrUtils.GetTranslation(ref topHintTooltip); }
            static Dictionary<string, string> topHintTooltip = new() { { "en", "1. Tools - 2. WebSocket Server Settings - 3. Enable WebSocket server\n?. Optional 'Enable Authentication'. To find out the current password, click 'Show Connect Info'.\nDon't forget to click Apply after making changes to the server." }, { "fr", "1. Outils - 2. Paramètres du serveur WebSocket - 3. Activer le serveur WebSocket\n?. Optionnel 'Activer l'authentification'. Pour connaître le mot de passe actuel, cliquez sur « Afficher les informations de connexion ».\nN'oubliez pas de cliquer sur Appliquer après avoir apporté des modifications au serveur." }, { "ru", "1. Сервис - 2. Настройки сервера WebSocket - 3. Включить сервер WebSocket\n?. Опционально 'Включить аутентификацию'. Чтобы узнать текущий пароль, нажмите 'Показать сведения о подключении'.\nНе забывайте нажимать кнопку 'Применить' после внесения изменений в сервер." } };

            /// <summary>
            /// "Support the developer!"
            /// </summary>
            public static string SupportDeveloper { get => TrUtils.GetTranslation(ref supportDeveloper); }
            static Dictionary<string, string> supportDeveloper = new() { { "en", "Support the developer!" }, { "fr", "Soutenez le développeur !" }, { "ru", "Поддержать разработчика!" }, { "zh-CN", "支持开发者！" } };

            /// <summary>
            /// "Report a bug or suggest a feature."
            /// </summary>
            public static string ReportBug { get => TrUtils.GetTranslation(ref reportBug); }
            static Dictionary<string, string> reportBug = new() { { "en", "Report a bug or suggest a feature." }, { "fr", "Signaler un bug ou suggérer une fonctionnalité." }, { "ru", "Сообщить об ошибке или предложить функцию." }, { "zh-CN", "报告错误或推荐功能。" } };

            /// <summary>
            /// "Server address:"
            /// </summary>
            public static string ServerAddressLabel { get => TrUtils.GetTranslation(ref serverAddressLabel); }
            static Dictionary<string, string> serverAddressLabel = new() { { "en", "Server address:" }, { "fr", "Adresse du serveur:" }, { "ru", "Адрес сервера:" }, { "zh-CN", "WebSocket服务器地址：" } };

            /// <summary>
            /// "Password:"
            /// </summary>
            public static string PasswordLabel { get => TrUtils.GetTranslation(ref passwordLabel); }
            static Dictionary<string, string> passwordLabel = new() { { "en", "Password:" }, { "fr", "Mot de passe :" }, { "ru", "Пароль:" }, { "zh-CN", "服务器密码：" } };

            /// <summary>
            /// "The password will be saved in a local file"
            /// </summary>
            public static string PasswordWarning { get => TrUtils.GetTranslation(ref passwordWarning); }
            static Dictionary<string, string> passwordWarning = new() { { "en", "The password will be saved in a local file" }, { "fr", "Le mot de passe sera sauvegardé dans un fichier local" }, { "ru", "Пароль будет сохранен в локальном файле" }, { "zh-CN", "密码将保存到本地文件" } };

            /// <summary>
            /// "Connect"
            /// </summary>
            public static string ConnectButtonConnect { get => TrUtils.GetTranslation(ref connectButtonConnect); }
            static Dictionary<string, string> connectButtonConnect = new() { { "en", "Connect" }, { "fr", "Connecter" }, { "ru", "Подключить" }, { "zh-CN", "连接" } };

            /// <summary>
            /// "Disconnect"
            /// </summary>
            public static string ConnectButtonDisconnect { get => TrUtils.GetTranslation(ref connectButtonDisconnect); }
            static Dictionary<string, string> connectButtonDisconnect = new() { { "en", "Disconnect" }, { "fr", "Déconnecter" }, { "ru", "Отключить" }, { "zh-CN", "断开连接" } };

            /// <summary>
            /// "Trying to reconnect... Cancel?"
            /// </summary>
            public static string ConnectButtonTryingToReconnect { get => TrUtils.GetTranslation(ref connectButtonTryingToReconnect); }
            static Dictionary<string, string> connectButtonTryingToReconnect = new() { { "en", "Trying to reconnect... Cancel?" }, { "fr", "Tentative de reconnexion... Annuler ?" }, { "ru", "Попытка переподключения... Отменить?" }, { "zh-CN", "正在尝试重新连接/单击取消" } };

            /// <summary>
            /// "Run at Windows startup"
            /// </summary>
            public static string RunWithWindows { get => TrUtils.GetTranslation(ref runWithWindows); }
            static Dictionary<string, string> runWithWindows = new() { { "en", "Run at Windows startup" }, { "fr", "Exécuter au démarrage de Windows" }, { "ru", "Запускать вместе с Windows" }, { "zh-CN", "开机启动" } };

            /// <summary>
            /// "(unavailable due to an error.\nTry to run as administrator)"
            /// </summary>
            public static string RunWithWindowsAdminError { get => TrUtils.GetTranslation(ref runWithWindowsAdminError); }
            static Dictionary<string, string> runWithWindowsAdminError = new() { { "en", "(unavailable due to an error.\nTry to run as administrator)" }, { "fr", "(indisponible en raison d'une erreur.\nEssayez d'exécuter en tant qu'administrateur)" }, { "ru", "(недоступно из-за ошибки.\nПопробуйте запустить от имени администратора)" }, { "zh-CN", "(由于未知错误而不可用。尝试以管理员身份运行)" } };

            /// <summary>
            /// "(a different path is used now)"
            /// </summary>
            public static string RunWithWindowsDifferentPath { get => TrUtils.GetTranslation(ref runWithWindowsDifferentPath); }
            static Dictionary<string, string> runWithWindowsDifferentPath = new() { { "en", "(a different path is used now)" }, { "fr", "(un chemin différent est utilisé maintenant)" }, { "ru", "(сейчас используется другой путь)" }, { "zh-CN", "(现在使用不同的路径)" } };

            /// <summary>
            /// "Create or update a script to run the program automatically."
            /// </summary>
            public static string RunWithObsHint { get => TrUtils.GetTranslation(ref runWithObsHint); }
            static Dictionary<string, string> runWithObsHint = new() { { "en", "Create or update a script to run the program automatically." }, { "fr", "Créez ou mettez à jour un script pour exécuter le programme automatiquement." }, { "ru", "Создание или обновление скрипта для автоматического запуска программы." }, { "zh-CN", "创建或更新脚本以自动运行程序。" } };

            /// <summary>
            /// "The script does not match the current version,\nor the registered path to {0} does not match the current program path."
            /// </summary>
            public static string RunWithObsHintOutdated(string arg0) => string.Format(TrUtils.GetTranslation(ref runWithObsHintOutdated), arg0);
            static Dictionary<string, string> runWithObsHintOutdated = new() { { "en", "The script does not match the current version,\nor the registered path to {0} does not match the current program path." }, { "fr", "Le script ne correspond pas à la version actuelle,\nou le chemin enregistré vers {0} ne correspond pas au chemin du programme actuel." }, { "ru", "Скрипт не совпадает с текущей версией,\nили зарегистрированный путь к {0} не соответствует текущему пути программы." }, { "zh-CN", "脚本不匹配当前版本，\n或 {0} 注册路径与当前程序路径不匹配。" } };

            /// <summary>
            /// "Start with OBS"
            /// </summary>
            public static string RunWithObsButton { get => TrUtils.GetTranslation(ref runWithObsButton); }
            static Dictionary<string, string> runWithObsButton = new() { { "en", "Start with OBS" }, { "fr", "Commencer par OBS" }, { "ru", "Запуск с OBS" }, { "zh-CN", "从OBS开始" } };

            /// <summary>
            /// "(script is outdated)"
            /// </summary>
            public static string RunWithObsButtonOutdated { get => TrUtils.GetTranslation(ref runWithObsButtonOutdated); }
            static Dictionary<string, string> runWithObsButtonOutdated = new() { { "en", "(script is outdated)" }, { "fr", "(le script est obsolète)" }, { "ru", "(скрипт устарел)" }, { "zh-CN", "(脚本已过时/失效)" } };

            /// <summary>
            /// "Close this program when exiting OBS\n(when Settings window closed)"
            /// </summary>
            public static string CloseOnObsExit { get => TrUtils.GetTranslation(ref closeOnObsExit); }
            static Dictionary<string, string> closeOnObsExit = new() { { "en", "Close this program when exiting OBS\n(when Settings window closed)" }, { "fr", "Fermer ce programme lorsque vous quittez OBS\n(lorsque la fenêtre Paramètres est fermée)" }, { "ru", "Закрывать программу при выключении OBS\n(когда окно настроек закрыто)" }, { "zh-CN", "退出OBS\n时关闭此程序 (设置窗口关闭时)" } };

            /// <summary>
            /// "Notification Settings"
            /// </summary>
            public static string NotificationSettings { get => TrUtils.GetTranslation(ref notificationSettings); }
            static Dictionary<string, string> notificationSettings = new() { { "en", "Notification Settings" }, { "fr", "Paramètres de notification" }, { "ru", "Настройки уведомлений" }, { "zh-CN", "通知设置" } };

            /// <summary>
            /// "Preview"
            /// </summary>
            public static string Preview { get => TrUtils.GetTranslation(ref preview); }
            static Dictionary<string, string> preview = new() { { "en", "Preview" }, { "fr", "Aperçu" }, { "ru", "Предпросмотр" }, { "zh-CN", "预览通知提示位置" } };

            /// <summary>
            /// "Screen:"
            /// </summary>
            public static string ScreenLabel { get => TrUtils.GetTranslation(ref screenLabel); }
            static Dictionary<string, string> screenLabel = new() { { "en", "Screen:" }, { "fr", "Ecran:" }, { "ru", "Экран:" }, { "zh-CN", "屏幕：" } };

            /// <summary>
            /// "Active modules:"
            /// </summary>
            public static string ActiveModulesLabel { get => TrUtils.GetTranslation(ref activeModulesLabel); }
            static Dictionary<string, string> activeModulesLabel = new() { { "en", "Active modules:" } };

            /// <summary>
            /// "Module settings:"
            /// </summary>
            public static string ModuleSettingsLabel { get => TrUtils.GetTranslation(ref moduleSettingsLabel); }
            static Dictionary<string, string> moduleSettingsLabel = new() { { "en", "Module settings:" } };

            /// <summary>
            /// "Select Active Notifications"
            /// </summary>
            public static string SelectActiveNotificationsButton { get => TrUtils.GetTranslation(ref selectActiveNotificationsButton); }
            static Dictionary<string, string> selectActiveNotificationsButton = new() { { "en", "Select Active Notifications" }, { "fr", "Sélectionnez les notifications actives" }, { "ru", "Выбрать активные уведомления" }, { "zh-CN", "选择活动通知" } };

            /// <summary>
            /// "Use safe display area\n(without the taskbar and other panels)"
            /// </summary>
            public static string UseSafeZone { get => TrUtils.GetTranslation(ref useSafeZone); }
            static Dictionary<string, string> useSafeZone = new() { { "en", "Use safe display area\n(without the taskbar and other panels)" }, { "fr", "Utiliser la zone d'affichage sécurisée\n(sans la barre des tâches et d'autres panneaux)" }, { "ru", "Использовать безопасную область экрана\n(без панели задач и других панелей)" }, { "zh-CN", "使用安全显示区域\n(没有任务栏和其他面板)" } };

            /// <summary>
            /// "Style Options:"
            /// </summary>
            public static string StyleOptionsLabel { get => TrUtils.GetTranslation(ref styleOptionsLabel); }
            static Dictionary<string, string> styleOptionsLabel = new() { { "en", "Style Options:" }, { "fr", "Options de style :" }, { "ru", "Варианты стиля:" }, { "zh-CN", "选择通知在屏幕的左侧或右侧" } };

            /// <summary>
            /// "Reset"
            /// </summary>
            public static string ResetHint { get => TrUtils.GetTranslation(ref resetHint); }
            static Dictionary<string, string> resetHint = new() { { "en", "Reset" }, { "ru", "Сбросить" }, { "zh-CN", "重置" } };

            /// <summary>
            /// "Reset to the center"
            /// </summary>
            public static string ResetCenterHint { get => TrUtils.GetTranslation(ref resetCenterHint); }
            static Dictionary<string, string> resetCenterHint = new() { { "en", "Reset to the center" }, { "fr", "Réinitialiser au centre" }, { "ru", "Сбросить в центр" }, { "zh-CN", "重置到中心" } };

            /// <summary>
            /// "Fix config"
            /// </summary>
            public static string FixConfigHint { get => TrUtils.GetTranslation(ref fixConfigHint); }
            static Dictionary<string, string> fixConfigHint = new() { { "en", "Fix config" }, { "fr", "Corriger la configuration" }, { "ru", "Исправить настройки" }, { "zh-CN", "修复配置" } };

            /// <summary>
            /// "Position Offset:"
            /// </summary>
            public static string PositionOffsetLabel { get => TrUtils.GetTranslation(ref positionOffsetLabel); }
            static Dictionary<string, string> positionOffsetLabel = new() { { "en", "Position Offset:" }, { "fr", "Décalage de position:" }, { "ru", "Смещение:" }, { "zh-CN", "上下/左右位置偏移量：" } };

            /// <summary>
            /// "Fade Delay (Seconds):"
            /// </summary>
            public static string FadeDelayLabel { get => TrUtils.GetTranslation(ref fadeDelayLabel); }
            static Dictionary<string, string> fadeDelayLabel = new() { { "en", "Fade Delay (Seconds):" }, { "fr", "Délai de fondu (secondes):" }, { "ru", "Задержка затухания (сек):" }, { "zh-CN", "淡出延迟 (秒):" } };

            /// <summary>
            /// "Delay before hiding notification"
            /// </summary>
            public static string FadeDelayHint { get => TrUtils.GetTranslation(ref fadeDelayHint); }
            static Dictionary<string, string> fadeDelayHint = new() { { "en", "Delay before hiding notification" }, { "fr", "Délai avant de masquer la notification" }, { "ru", "Задержка перед скрытием уведомления" }, { "zh-CN", "隐藏通知前延迟" } };

            /// <summary>
            /// "Additional:"
            /// </summary>
            public static string AdditionalLabel { get => TrUtils.GetTranslation(ref additionalLabel); }
            static Dictionary<string, string> additionalLabel = new() { { "en", "Additional:" }, { "fr", "En plus:" }, { "ru", "Дополнительно:" }, { "zh-CN", "附加：" } };

            /// <summary>
            /// "Open Module Settings"
            /// </summary>
            public static string OpenModuleSettings { get => TrUtils.GetTranslation(ref openModuleSettings); }
            static Dictionary<string, string> openModuleSettings = new() { { "en", "Open Module Settings" }, { "fr", "Ouvrir les paramètres du module" }, { "ru", "Открыть настройки модуля" }, { "zh-CN", "打开模块设置" } };

        }

        public static class ActiveNotifications
        {
            /// <summary>
            /// "Select Active Notifications"
            /// </summary>
            public static string Title { get => TrUtils.GetTranslation(ref title); }
            static Dictionary<string, string> title = new() { { "en", "Select Active Notifications" }, { "ru", "Выберите активные уведомления" }, { "zh-CN", "选择活动通知" } };

            /// <summary>
            /// "Select All"
            /// </summary>
            public static string SelectAll { get => TrUtils.GetTranslation(ref selectAll); }
            static Dictionary<string, string> selectAll = new() { { "en", "Select All" }, { "ru", "Выбрать всё" }, { "zh-CN", "选择所有" } };

            /// <summary>
            /// "Select None"
            /// </summary>
            public static string SelectNone { get => TrUtils.GetTranslation(ref selectNone); }
            static Dictionary<string, string> selectNone = new() { { "en", "Select None" }, { "ru", "Очистить" }, { "zh-CN", "取消全选" } };

            /// <summary>
            /// "Reset to current"
            /// </summary>
            public static string ResetToCurrent { get => TrUtils.GetTranslation(ref resetToCurrent); }
            static Dictionary<string, string> resetToCurrent = new() { { "en", "Reset to current" }, { "ru", "Восстановить" }, { "zh-CN", "重置为保存的设置" } };

            /// <summary>
            /// "Reset to default"
            /// </summary>
            public static string ResetToDefault { get => TrUtils.GetTranslation(ref resetToDefault); }
            static Dictionary<string, string> resetToDefault = new() { { "en", "Reset to default" }, { "ru", "По умолчанию" }, { "zh-CN", "重置为默认值" } };

            /// <summary>
            /// "Save"
            /// </summary>
            public static string Save { get => TrUtils.GetTranslation(ref save); }
            static Dictionary<string, string> save = new() { { "en", "Save" }, { "ru", "Сохранить" }, { "zh-CN", "保存" } };

        }

        public static class TrayMenu
        {
            /// <summary>
            /// "Open Settings"
            /// </summary>
            public static string OpenSettings { get => TrUtils.GetTranslation(ref openSettings); }
            static Dictionary<string, string> openSettings = new() { { "en", "Open Settings" }, { "ru", "Открыть настройки" }, { "zh-CN", "打开设置" } };

            /// <summary>
            /// "Language"
            /// </summary>
            public static string Languages { get => TrUtils.GetTranslation(ref languages); }
            static Dictionary<string, string> languages = new() { { "en", "Language" }, { "ru", "Язык" }, { "zh-CN", "语言" } };

            /// <summary>
            /// "Translation progress (translated % / verified %)"
            /// </summary>
            public static string LanguagesCompletionHint { get => TrUtils.GetTranslation(ref languagesCompletionHint); }
            static Dictionary<string, string> languagesCompletionHint = new() { { "en", "Translation progress (translated % / verified %)" }, { "ru", "Прогресс перевода (переведено % / проверено %)" }, { "zh-CN", "翻译进度 (%/已验证%)" } };

            /// <summary>
            /// "{0} ({1}% / {2}%)"
            /// </summary>
            public static string LanguagesCompletionTemplate(string arg0, string arg1, string arg2) => string.Format(TrUtils.GetTranslation(ref languagesCompletionTemplate), arg0, arg1, arg2);
            static Dictionary<string, string> languagesCompletionTemplate = new() { { "en", "{0} ({1}% / {2}%)" } };

            /// <summary>
            /// "Check for updates"
            /// </summary>
            public static string CheckUpdates { get => TrUtils.GetTranslation(ref checkUpdates); }
            static Dictionary<string, string> checkUpdates = new() { { "en", "Check for updates" }, { "ru", "Проверить обновления" }, { "zh-CN", "检查更新" } };

            /// <summary>
            /// "Open logs folder"
            /// </summary>
            public static string OpenLogsFolder { get => TrUtils.GetTranslation(ref openLogsFolder); }
            static Dictionary<string, string> openLogsFolder = new() { { "en", "Open logs folder" }, { "ru", "Открыть папку логов" }, { "zh-CN", "打开日志文件夹" } };

            /// <summary>
            /// "About {0}"
            /// </summary>
            public static string About(string arg0) => string.Format(TrUtils.GetTranslation(ref about), arg0);
            static Dictionary<string, string> about = new() { { "en", "About {0}" }, { "ru", "О программе {0}" }, { "zh-CN", "关于 {0}" } };

            /// <summary>
            /// "Exit"
            /// </summary>
            public static string Exit { get => TrUtils.GetTranslation(ref exit); }
            static Dictionary<string, string> exit = new() { { "en", "Exit" }, { "ru", "Выход" }, { "zh-CN", "退出" } };

            /// <summary>
            /// "Connected"
            /// </summary>
            public static string StatusConnected { get => TrUtils.GetTranslation(ref statusConnected); }
            static Dictionary<string, string> statusConnected = new() { { "en", "Connected" }, { "ru", "Подключен" }, { "zh-CN", "已连接" } };

            /// <summary>
            /// "Not connected"
            /// </summary>
            public static string StatusNotConnected { get => TrUtils.GetTranslation(ref statusNotConnected); }
            static Dictionary<string, string> statusNotConnected = new() { { "en", "Not connected" }, { "ru", "Не подключен" }, { "zh-CN", "未连接" } };

            /// <summary>
            /// "Trying to reconnect"
            /// </summary>
            public static string StatusTryingToReconnect { get => TrUtils.GetTranslation(ref statusTryingToReconnect); }
            static Dictionary<string, string> statusTryingToReconnect = new() { { "en", "Trying to reconnect" }, { "ru", "Попытка повторного подключения" }, { "zh-CN", "正在尝试重新连接" } };

        }

        public static class NotificationEvents
        {
            /// <summary>
            /// "Open folder"
            /// </summary>
            public static string FileSaveOpenFolder { get => TrUtils.GetTranslation(ref fileSaveOpenFolder); }
            static Dictionary<string, string> fileSaveOpenFolder = new() { { "en", "Open folder" }, { "ru", "Открыть папку" }, { "zh-CN", "打开文件夹" } };

            /// <summary>
            /// "Open file"
            /// </summary>
            public static string FileSaveOpenFile { get => TrUtils.GetTranslation(ref fileSaveOpenFile); }
            static Dictionary<string, string> fileSaveOpenFile = new() { { "en", "Open file" }, { "ru", "Открыть файл" }, { "zh-CN", "打开文件" } };

            /// <summary>
            /// "Preview Notification"
            /// </summary>
            public static string Preview { get => TrUtils.GetTranslation(ref preview); }
            static Dictionary<string, string> preview = new() { { "en", "Preview Notification" }, { "ru", "Предпросмотр уведомления" }, { "zh-CN", "预览通知" } };

            /// <summary>
            /// "Some description"
            /// </summary>
            public static string Preview2ndLine { get => TrUtils.GetTranslation(ref preview2ndLine); }
            static Dictionary<string, string> preview2ndLine = new() { { "en", "Some description" }, { "ru", "Некое описание" }, { "zh-CN", "一些描述" } };

            /// <summary>
            /// "Connected to OBS"
            /// </summary>
            public static string Connected { get => TrUtils.GetTranslation(ref connected); }
            static Dictionary<string, string> connected = new() { { "en", "Connected to OBS" }, { "ru", "Подключено к OBS" }, { "zh-CN", "已连接到OBS" } };

            /// <summary>
            /// "Disconnected from OBS"
            /// </summary>
            public static string Disconnected { get => TrUtils.GetTranslation(ref disconnected); }
            static Dictionary<string, string> disconnected = new() { { "en", "Disconnected from OBS" }, { "ru", "Отключено от OBS" }, { "zh-CN", "从OBS断开连接" } };

            /// <summary>
            /// "Lost Connection to OBS"
            /// </summary>
            public static string LostConnection { get => TrUtils.GetTranslation(ref lostConnection); }
            static Dictionary<string, string> lostConnection = new() { { "en", "Lost Connection to OBS" }, { "ru", "Потеряно соединение с OBS" }, { "zh-CN", "与OBS的连接丢失" } };

            /// <summary>
            /// "Trying to reconnect"
            /// </summary>
            public static string LostConnection2ndLine { get => TrUtils.GetTranslation(ref lostConnection2ndLine); }
            static Dictionary<string, string> lostConnection2ndLine = new() { { "en", "Trying to reconnect" }, { "ru", "Попытка переподключения" }, { "zh-CN", "尝试重新连接" } };

            /// <summary>
            /// "Replay Started"
            /// </summary>
            public static string ReplayStarted { get => TrUtils.GetTranslation(ref replayStarted); }
            static Dictionary<string, string> replayStarted = new() { { "en", "Replay Started" }, { "ru", "Повтор запущен" }, { "zh-CN", "重播已开始" } };

            /// <summary>
            /// "Replay Stopped"
            /// </summary>
            public static string ReplayStopped { get => TrUtils.GetTranslation(ref replayStopped); }
            static Dictionary<string, string> replayStopped = new() { { "en", "Replay Stopped" }, { "ru", "Повтор остановлен" }, { "zh-CN", "回放已停止" } };

            /// <summary>
            /// "Replay Saved"
            /// </summary>
            public static string ReplaySaved { get => TrUtils.GetTranslation(ref replaySaved); }
            static Dictionary<string, string> replaySaved = new() { { "en", "Replay Saved" }, { "ru", "Повтор сохранён" }, { "zh-CN", "回放已保存" } };

            /// <summary>
            /// "Recording Started"
            /// </summary>
            public static string RecordingStarted { get => TrUtils.GetTranslation(ref recordingStarted); }
            static Dictionary<string, string> recordingStarted = new() { { "en", "Recording Started" }, { "ru", "Запись началась" }, { "zh-CN", "录制已开始" } };

            /// <summary>
            /// "Recording Stopped"
            /// </summary>
            public static string RecordingStopped { get => TrUtils.GetTranslation(ref recordingStopped); }
            static Dictionary<string, string> recordingStopped = new() { { "en", "Recording Stopped" }, { "ru", "Запись остановлена" }, { "zh-CN", "录制已停止" } };

            /// <summary>
            /// "Recording Paused"
            /// </summary>
            public static string RecordingPaused { get => TrUtils.GetTranslation(ref recordingPaused); }
            static Dictionary<string, string> recordingPaused = new() { { "en", "Recording Paused" }, { "ru", "Запись приостановлена" }, { "zh-CN", "录制已暂停" } };

            /// <summary>
            /// "Recording Resumed"
            /// </summary>
            public static string RecordingResumed { get => TrUtils.GetTranslation(ref recordingResumed); }
            static Dictionary<string, string> recordingResumed = new() { { "en", "Recording Resumed" }, { "ru", "Запись возобновлена" }, { "zh-CN", "录制已恢复" } };

            /// <summary>
            /// "Streaming Started"
            /// </summary>
            public static string StreamingStarted { get => TrUtils.GetTranslation(ref streamingStarted); }
            static Dictionary<string, string> streamingStarted = new() { { "en", "Streaming Started" }, { "ru", "Трансляция началась" }, { "zh-CN", "流媒体/直播已开始" } };

            /// <summary>
            /// "Streaming Stopped"
            /// </summary>
            public static string StreamingStopped { get => TrUtils.GetTranslation(ref streamingStopped); }
            static Dictionary<string, string> streamingStopped = new() { { "en", "Streaming Stopped" }, { "ru", "Трансляция остановлена" }, { "zh-CN", "流媒体/直播已停止" } };

            /// <summary>
            /// "Virtual Camera Started"
            /// </summary>
            public static string VirtualCameraStarted { get => TrUtils.GetTranslation(ref virtualCameraStarted); }
            static Dictionary<string, string> virtualCameraStarted = new() { { "en", "Virtual Camera Started" }, { "ru", "Виртуальная камера запущена" }, { "zh-CN", "虚拟摄像机已启动" } };

            /// <summary>
            /// "Virtual Camera Stopped"
            /// </summary>
            public static string VirtualCameraStopped { get => TrUtils.GetTranslation(ref virtualCameraStopped); }
            static Dictionary<string, string> virtualCameraStopped = new() { { "en", "Virtual Camera Stopped" }, { "ru", "Виртуальная камера остановлена" }, { "zh-CN", "虚拟相机已停止" } };

            /// <summary>
            /// "Scene Switched"
            /// </summary>
            public static string SceneSwitched { get => TrUtils.GetTranslation(ref sceneSwitched); }
            static Dictionary<string, string> sceneSwitched = new() { { "en", "Scene Switched" }, { "ru", "Сцена переключена" }, { "zh-CN", "已切换场景" } };

            /// <summary>
            /// "Current: {0}"
            /// </summary>
            public static string SceneSwitched2ndLine(string arg0) => string.Format(TrUtils.GetTranslation(ref sceneSwitched2ndLine), arg0);
            static Dictionary<string, string> sceneSwitched2ndLine = new() { { "en", "Current: {0}" }, { "ru", "Текущая: {0}" }, { "zh-CN", "当前: {0}" } };

            /// <summary>
            /// "Scene Collection Switched"
            /// </summary>
            public static string SceneCollectionSwitched { get => TrUtils.GetTranslation(ref sceneCollectionSwitched); }
            static Dictionary<string, string> sceneCollectionSwitched = new() { { "en", "Scene Collection Switched" }, { "ru", "Коллекция сцен переключена" }, { "zh-CN", "场景集合切换成功" } };

            /// <summary>
            /// "Current: {0}"
            /// </summary>
            public static string SceneCollectionSwitched2ndLine(string arg0) => string.Format(TrUtils.GetTranslation(ref sceneCollectionSwitched2ndLine), arg0);
            static Dictionary<string, string> sceneCollectionSwitched2ndLine = new() { { "en", "Current: {0}" }, { "ru", "Текущая: {0}" }, { "zh-CN", "当前: {0}" } };

            /// <summary>
            /// "Profile Switched"
            /// </summary>
            public static string ProfileSwitched { get => TrUtils.GetTranslation(ref profileSwitched); }
            static Dictionary<string, string> profileSwitched = new() { { "en", "Profile Switched" }, { "ru", "Профиль переключен" }, { "zh-CN", "已切换配置文件" } };

            /// <summary>
            /// "Current: {0}"
            /// </summary>
            public static string ProfileSwitched2ndLine(string arg0) => string.Format(TrUtils.GetTranslation(ref profileSwitched2ndLine), arg0);
            static Dictionary<string, string> profileSwitched2ndLine = new() { { "en", "Current: {0}" }, { "ru", "Текущий: {0}" }, { "zh-CN", "当前: {0}" } };

            /// <summary>
            /// "Audio is Muted"
            /// </summary>
            public static string AudioMuted { get => TrUtils.GetTranslation(ref audioMuted); }
            static Dictionary<string, string> audioMuted = new() { { "en", "Audio is Muted" }, { "ru", "Звук отключен" }, { "zh-CN", "音频已静音" } };

            /// <summary>
            /// "Audio is Turned On"
            /// </summary>
            public static string AudioTurnedOn { get => TrUtils.GetTranslation(ref audioTurnedOn); }
            static Dictionary<string, string> audioTurnedOn = new() { { "en", "Audio is Turned On" }, { "ru", "Звук включен" }, { "zh-CN", "音频已打开" } };

            /// <summary>
            /// "Source: {0}"
            /// </summary>
            public static string Audio2ndLine(string arg0) => string.Format(TrUtils.GetTranslation(ref audio2ndLine), arg0);
            static Dictionary<string, string> audio2ndLine = new() { { "en", "Source: {0}" }, { "ru", "Источник: {0}" }, { "zh-CN", "资料来源： {0}" } };

            /// <summary>
            /// "Screenshot saved"
            /// </summary>
            public static string ScreenshotSaved { get => TrUtils.GetTranslation(ref screenshotSaved); }
            static Dictionary<string, string> screenshotSaved = new() { { "en", "Screenshot saved" }, { "ru", "Скриншот сохранён" }, { "zh-CN", "截图已保存" } };

        }

        public static class Modules
        {
            /// <summary>
            /// "Stack of notifications"
            /// </summary>
            public static string DefaultModuleName { get => TrUtils.GetTranslation(ref defaultModuleName); }
            static Dictionary<string, string> defaultModuleName = new() { { "en", "Stack of notifications" }, { "ru", "Стопка уведомлений" }, { "zh-CN", "堆栈通知" } };

            /// <summary>
            /// "This is the default notification module.\nIt displays one or more notifications located in the corner of the monitor."
            /// </summary>
            public static string DefaultModuleDesc { get => TrUtils.GetTranslation(ref defaultModuleDesc); }
            static Dictionary<string, string> defaultModuleDesc = new() { { "en", "This is the default notification module.\nIt displays one or more notifications located in the corner of the monitor." }, { "ru", "Это модуль уведомлений по умолчанию.\nОн отображает одно или несколько уведомлений, расположенных в углу монитора." }, { "zh-CN", "这是默认的通知模块。\n它显示一个或多个位于监视器角落的通知。" } };

            /// <summary>
            /// "Nvidia-Like"
            /// </summary>
            public static string NvidiaLikeModuleName { get => TrUtils.GetTranslation(ref nvidiaLikeModuleName); }
            static Dictionary<string, string> nvidiaLikeModuleName = new() { { "en", "Nvidia-Like" }, { "ru", "Как у Nvidia" }, { "zh-CN", "Nvidia风格的通知样式" } };

            /// <summary>
            /// "Simple notification similar to Nvidia's notifications"
            /// </summary>
            public static string NvidiaLikeModuleDesc { get => TrUtils.GetTranslation(ref nvidiaLikeModuleDesc); }
            static Dictionary<string, string> nvidiaLikeModuleDesc = new() { { "en", "Simple notification similar to Nvidia's notifications" }, { "ru", "Простое уведомление, похожее на уведомления Nvidia" }, { "zh-CN", "类似于Nvidia通知的简单通知" } };

        }

    }
}
