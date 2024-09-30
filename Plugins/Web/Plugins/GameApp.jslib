var GameApp = {
    $gameApp: {
        onVisibilityChanged: function () { },

        boundVisibilityChange: null,
        boundSendVisible: null,
        boundSendHidden: null,
        
        VisibilityHandlerInitialize: function (onVisibilityChanged) {
            this.onVisibilityChanged = onVisibilityChanged;

            if (this.boundVisibilityChange)
                document.removeEventListener('visibilitychange', this.boundVisibilityChange);
            if (this.boundSendVisible)
                document.removeEventListener('blur', this.boundSendVisible);
            if (this.boundSendHidden)
                document.removeEventListener('focus', this.boundSendHidden);

            this.boundVisibilityChange = this.OnVisibilityChanged.bind(this);
            this.boundSendVisible = this.SendVisible.bind(this);
            this.boundSendHidden = this.SendHidden.bind(this);

            document.addEventListener('visibilitychange', this.boundVisibilityChange);
            document.addEventListener('blur', this.boundSendVisible);
            document.addEventListener('focus', this.boundSendHidden);
        },

        OnVisibilityChanged: function ()
        {
            if(document.hidden) {
                this.SendHidden();
            } else {
                this.SendVisible();
            }
        },

        SendVisible: function ()
        {
            if(this.onVisibilityChanged === undefined)
                return;

            dynCall('vi', this.onVisibilityChanged, [true]);
        },

        SendHidden: function ()
        {
            if(gameApp.onVisibilityChanged === undefined)
                return;

            dynCall('vi', this.onVisibilityChanged, [false]);
        },
    },

    GameAppVisibilityHandler: function (onVisibilityChanged) {
        gameApp.VisibilityHandlerInitialize(onVisibilityChanged);
    },
}

autoAddDeps(GameApp, '$gameApp');
mergeInto(LibraryManager.library, GameApp);