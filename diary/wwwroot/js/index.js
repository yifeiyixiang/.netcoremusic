$(function(){
    var playerContent1 = $('#player-content1');// 歌曲信息模块部分dom元素
    const ulmyList = document.getElementById('myList'); 
    var musicName = $('.music-name');          // 歌曲名部分dom元素 
	var musicList = $('.music-list');          // 歌曲名list部分dom元素
    var artistName = $('.artist-name');        // 歌手名部分dom元素
    musicList.addClass('active2');// 内容栏上移
    var musicImgs = $('.music-imgs');          // 左侧封面图dom元素
  
    var playPauseBtn = $('.play-pause');       // 播放/暂停按钮 dom元素
    var playPrevBtn = $('.prev');              // 上一首按钮 dom元素
    var playNextBtn = $('.next')               // 下一首按钮 dom元素
    
    var time = $('.time');                     // 时间信息部分 dom元素
    var tProgress = $('.current-time');        // 当前播放时间文本部分 dom元素
    var totalTime = $('.total-time');          // 歌曲总时长文本部分 dom元素
    
    var sArea = $('#s-area');                  // 进度条部分
    var insTime = $('#ins-time');              // 鼠标移动至进度条上面，显示的信息部分
    var sHover = $('#s-hover');                // 鼠标移动至进度条上面，前面变暗的进度条部分
    var seekBar = $('#seek-bar');              // 播放进度条部分
    var currIndex = -1;              // 当前播放索引

    var buffInterval = null          // 初始化定时器 判断是否需要缓冲
    var loopmusic = false;  //单曲循环播放变量
    var randomusic = false;  //随机播放变量
    // 一些计算所需的变量
    var seekT, seekLoc, seekBarPos, cM, ctMinutes, ctSeconds, curMinutes, curSeconds, durMinutes, durSeconds, playProgress, bTime, nTime = 0
    var musicImgsData = ['/img/ChatGPT.png', '/img/ChatGPT.png','/img/avatar.png']    // 图片地址数组
    // var musicNameData = ['你怎么能够','爱与妒忌','残忍的温柔'];                   // 歌曲名数组 
    // var artistNameData = ['花粥/王胜娚','花粥/马雨阳','花粥']            // 创作歌手数组
    //var musicUrls = ['/music/别说我的眼泪你无所谓.mp3', '/music/听悲伤的情歌 - 什么琪.mp3', '/music/兄弟抱一下-丁呱呱.mp3', '/music/真情永不变-花姐.mp3'];// 歌曲mp3数组
    var musicUrls = [];
        //$.ajax({
        //    url: "/Account/GetMusicsrc", // 修改为获取数据的后端接口地址
        //    success: function (data) { 
        //        musicUrls = JSON.parse(data);
        //        console.log("musicUrls " + data);
        //        // 调用初始化函数
        //        initPlayer();
        //    },
        //    error: function () {
        //        console.log('Error occurred while fetching music data.');
        //    }
        //}); 
	var musicNameData = [];  
    //$.ajax({
    //    url: "/Account/GetMusicname", // 修改为获取数据的后端接口地址
    //    success: function (data) {
    //        musicNameData = JSON.parse(data); // 将返回的 JSON 字符串解析为数组 
    //        // 在这里执行需要在数据返回后进行的操作

    //        console.log("musicNameData " + musicNameData); 
    //        // 获取要插入的UL元素
    //        len = musicNameData.length;  // 歌曲长度
            
    //        // 遍历数组
    //        musicNameData.forEach(function (item) {
    //            // 创建一个新的LI元素
    //            const li = document.createElement('li');
    //            // 将数组元素赋值给LI元素的文本内容
    //            li.textContent = item; 
    //            // 获取要插入的UL元素 
    //            // 将LI元素添加到UL列表中
    //            ulmyList.appendChild(li);
    //        });

    //        // 获取所有的 li 元素
    //        const liElements = document.querySelectorAll('li');
    //        // 为每个 li 元素添加点击事件监听器
    //        liElements.forEach(function (li, index) {
    //            li.addEventListener('click', function () {
    //                console.log("点击的index： "+index)
    //                selectTrack2(index);
    //            });
    //        });

    //    },
    //    error: function () {
    //        console.log('Error occurred while fetching music data.');
    //    }
    //}); 
     
    var musicalbum = [];
    async function Getalbumdata() {
        try {
            const Getalbum = $.ajax({
                url: "/Account/Getalbum",
            });
            // 并行请求 musicUrls 和 musicNameData 的数据
            const musicsrcRes = await Promise.all([Getalbum]);
            musicalbum = JSON.parse(musicsrcRes[0]); // 注意这里取第一个元素
           
            // 循环遍历文件名数组
            var ul = document.createElement("ul");
            ul.setAttribute("id", "albumList");  // 设置id属性为"myList"
            //var li = document.createElement("li");
            //var textNode = document.createTextNode("歌单");
            //li.appendChild(textNode);
            //ul.appendChild(li); // 将新的 <li> 元素插入到 <ul> 元素
            musicalbum.forEach(function (musicObj) {
                var li = document.createElement("li");
                var textNode = document.createTextNode(musicObj.name);
                console.log("musicalbum歌单列表" + musicObj.name);
                li.appendChild(textNode); 
                ul.appendChild(li); // 将新的 <li> 元素插入到 <ul> 元素
                var playerContent2 = $('#player-content2');
                playerContent2.append(ul);

            });

            var albumList = document.getElementById('albumList');
            var listItems = albumList.getElementsByTagName('li'); 
            for (var i = 0; i < listItems.length; i++) {
                listItems[i].addEventListener('click', function () {
                    var clickedItem = this.innerHTML;
                    fetchData(clickedItem);
                    var firstItem = albumList.getElementsByTagName('li')[0]; 
                    // 获取第一个元素的内容
                    var firstItemContent = firstItem.innerHTML; 
                    // 将点击后的元素替换为第一个元素
                    firstItem.innerHTML = clickedItem; 
                    // 将被点击的元素替换为第一个元素的内容
                    this.innerHTML = firstItemContent;

                });
            }
            //首次加载也需要 调用 fetchData 函数，并将第一个 li 元素的内容作为参数传递
            const firstLiContent = listItems[0].innerHTML;
            fetchData(firstLiContent);
        } catch (error) {
            console.log('Error occurred while fetching Getalbum data.');
        }
    } 
    Getalbumdata();
    async function fetchData(albumName) {
        try {
            // 清空 ulmyList 的内容
            ulmyList.innerHTML = "";
            const getMusicsrc = $.ajax({
                url: "/Account/GetMusicsrc",
                data: { album: albumName }, // 使用传递进来的专辑名称作为参数
            });

            const getMusicname = $.ajax({
                url: "/Account/GetMusicname",
                data: { album: albumName }, // 使用传递进来的专辑名称作为参数
            });

            // 并行请求 musicUrls 和 musicNameData 的数据
            const [musicsrcRes, musicnameRes] = await Promise.all([
                getMusicsrc,
                getMusicname,
            ]);

            musicUrls = JSON.parse(musicsrcRes);
            musicNameData = JSON.parse(musicnameRes);

            // 初始化播放器
            initPlayer();

            // 在这里执行需要在数据返回后进行的操作
            console.log("musicNameData " + musicNameData);
            // 获取要插入的UL元素
            len = musicNameData.length; // 歌曲长度

            // 遍历数组
            musicNameData.forEach(function (item) {
                // 创建一个新的LI元素
                const li = document.createElement('li');
                // 将数组元素赋值给LI元素的文本内容
                li.textContent = item;
                // 获取要插入的UL元素 
                // 将LI元素添加到UL列表中
                ulmyList.appendChild(li);
            });

            // 获取所有的 li 元素
            const liElements = ulmyList.querySelectorAll('li');
            // 为每个 li 元素添加点击事件监听器
            liElements.forEach(function (li, index) {
                li.addEventListener('click', function () {
                    console.log("点击的index： " + index);
                    selectTrack2(index);
                });
            });
        } catch (error) {
            console.log('Error occurred while fetching music data.');
        }
    }

    // 调用异步函数
    //fetchData();
    var len = 0;  // 歌曲长度
 
  
    // 点击 播放/暂停 按钮，触发该函数
    // 作用：根据audio的paused属性 来检测当前音频是否已暂停  true:暂停  false:播放中
    function playPause(){
        if(audio.paused){
            playerContent1.addClass('active'); // 内容栏上移
            //musicList.addClass('active');// 内容栏上移
            musicImgs.addClass('active');      // 左侧图片开始动画效果
            playPauseBtn.attr('class','btn play-pause icon-zanting iconfont') // 显示暂停图标
            checkBuffering(); // 检测是否需要缓冲
            audio.play();     // 播放
        } else {
            //musicList.removeClass('active');// 内容栏下移
            playerContent1.removeClass('active'); // 内容栏下移
            musicImgs.removeClass('active');      // 左侧图片停止旋转等动画效果
            playPauseBtn.attr('class','btn play-pause icon-jiediankaishi iconfont'); // 显示播放按钮
            clearInterval(buffInterval);          // 清除检测是否需要缓冲的定时器
            musicImgs.removeClass('buffering');    // 移除缓冲类名
            audio.pause(); // 暂停
        }  
    }
    var musicControl = document.getElementById('musicControl');

    var flag = false;
    musicControl.addEventListener('click', function () {
        // 在这里编写点击事件的处理代码 
        if (flag) {
            musicList.addClass('active');// 内容栏上移
        } else {
            musicList.removeClass('active2');// 内容栏下移
            musicList.removeClass('active');// 内容栏下移 
        } 
        flag = !flag;
        //musicList.removeClass('active');// 内容栏下移
        //musicList.addClass('active');// 内容栏上移
    });

    // 鼠标移动在进度条上， 触发该函数	
	function showHover(event){
		seekBarPos = sArea.offset();    // 获取进度条长度
		seekT = event.clientX - seekBarPos.left;  //获取当前鼠标在进度条上的位置
		seekLoc = audio.duration * (seekT / sArea.outerWidth()); //当前鼠标位置的音频播放秒数： 音频长度(单位：s)*（鼠标在进度条上的位置/进度条的宽度）
		
		sHover.width(seekT);  //设置鼠标移动到进度条上变暗的部分宽度
		
		cM = seekLoc / 60;    // 计算播放了多少分钟： 音频播放秒速/60
		
		ctMinutes = Math.floor(cM);  // 向下取整
		ctSeconds = Math.floor(seekLoc - ctMinutes * 60); // 计算播放秒数
		
		if( (ctMinutes < 0) || (ctSeconds < 0) )
			return;
		
        if( (ctMinutes < 0) || (ctSeconds < 0) )
			return;
		
		if(ctMinutes < 10)
			ctMinutes = '0'+ctMinutes;
		if(ctSeconds < 10)
			ctSeconds = '0'+ctSeconds;
        
        if( isNaN(ctMinutes) || isNaN(ctSeconds) )
            insTime.text('--:--');
        else
		    insTime.text(ctMinutes+':'+ctSeconds);  // 设置鼠标移动到进度条上显示的信息
            
		insTime.css({'left':seekT,'margin-left':'-21px'}).fadeIn(0);  // 淡入效果显示
		
	}

    // 鼠标移出进度条，触发该函数
    function hideHover()
	{
        sHover.width(0);  // 设置鼠标移动到进度条上变暗的部分宽度 重置为0
        insTime.text('00:00').css({'left':'0px','margin-left':'0px'}).fadeOut(0); // 淡出效果显示
    }

    // 鼠标点击进度条，触发该函数
    function playFromClickedPos()
    {
        audio.currentTime = seekLoc; // 设置音频播放时间 为当前鼠标点击的位置时间
		seekBar.width(seekT);        // 设置进度条播放长度，为当前鼠标点击的长度
		hideHover();                 // 调用该函数，隐藏原来鼠标移动到上方触发的进度条阴影
    }

    // 在音频的播放位置发生改变是触发该函数
    function updateCurrTime()
	{
        nTime = new Date();      // 获取当前时间
        nTime = nTime.getTime(); // 将该时间转化为毫秒数

        // 计算当前音频播放的时间
		curMinutes = Math.floor(audio.currentTime  / 60);
        curSeconds = Math.floor(audio.currentTime  - curMinutes * 60);
        
		// 计算当前音频总时间
		durMinutes = Math.floor(audio.duration / 60);
        durSeconds = Math.floor(audio.duration - durMinutes * 60);
        
		// 计算播放进度百分比
		playProgress = (audio.currentTime  / audio.duration) * 100;
        
        // 如果时间为个位数，设置其格式
		if(curMinutes < 10)
			curMinutes = '0'+curMinutes;
		if(curSeconds < 10)
			curSeconds = '0'+curSeconds;
		
		if(durMinutes < 10)
			durMinutes = '0'+durMinutes;
		if(durSeconds < 10)
			durSeconds = '0'+durSeconds;
        
        if( isNaN(curMinutes) || isNaN(curSeconds) )
            tProgress.text('00:00');
        else
            tProgress.text(curMinutes+':'+curSeconds);
        
        if( isNaN(durMinutes) || isNaN(durSeconds) )
            totalTime.text('00:00');
        else
		    totalTime.text(durMinutes+':'+durSeconds);
        
        if( isNaN(curMinutes) || isNaN(curSeconds) || isNaN(durMinutes) || isNaN(durSeconds) )
            time.removeClass('active');
        else
            time.addClass('active');

        // 设置播放进度条的长度
		seekBar.width(playProgress+'%');
        
        // 进度条为100 即歌曲播放完时
		if( playProgress == 100 )
		{
            playPauseBtn.attr('class','btn play-pause icon-jiediankaishi iconfont'); // 显示播放按钮
			seekBar.width(0);              // 播放进度条重置为0
            tProgress.text('00:00');       // 播放时间重置为 00:00
            musicImgs.removeClass('buffering').removeClass('active');  // 移除相关类名
            clearInterval(buffInterval);   // 清除定时器

            selectTrack(1);  // 添加这一句，可以实现自动播放
		}
    }

    // 定时器检测是否需要缓冲
    function checkBuffering(){
        clearInterval(buffInterval);
        buffInterval = setInterval(function()
        { 
            // 这里如果音频播放了，则nTime为当前时间毫秒数，如果没播放则为0；如果时间间隔过长，也将缓存
            if( (nTime == 0) || (bTime - nTime) > 1000  ){ 
                musicImgs.addClass('buffering');  // 添加缓存样式类
            } else{
                musicImgs.removeClass('buffering'); // 移除缓存样式类
            }
                
            bTime = new Date();
            bTime = bTime.getTime();

        },100);
    }
   
    // 点击上一首/下一首时，触发该函数。 
    //注意：后面代码初始化时，会触发一次selectTrack(0)，因此下面一些地方需要判断flag是否为0
    function selectTrack(flag) {
        if (!loopmusic) {
            if (!randomusic) {
                if (flag == 0) {  // 初始 || 点击下一首 
                    currIndex = 0;
                } else if (flag == 1) {
                    ++currIndex;
                    console.log("点击下一首++后的：" + currIndex)
                    if (currIndex >= len) {      // 当处于最后一首时，点击下一首，播放索引置为第一首
                        currIndex = 0;
                    }
                } else {                    // 点击上一首
                    console.log(currIndex)
                    --currIndex;
                    console.log("点击上一首--后的：" + currIndex)
                    if (currIndex <= -1) {    // 当处于第一首时，点击上一首，播放索引置为最后一首
                        currIndex = len - 1;
                    }
                }
            } else {
                // 生成一个范围在 0 到 (len-1) 之间的随机整数
                var randomIndex = Math.floor(Math.random() * len);
                currIndex = randomIndex;
            }
        }
        console.log(currIndex)
        if (audio.paused){
            playPauseBtn.attr('class','btn play-pause icon-jiediankaishi iconfont'); // 显示播放图标
        }else{
            musicImgs.removeClass('buffering');   
            playPauseBtn.attr('class','btn play-pause icon-zanting iconfont') // 显示暂停图标
        }

        seekBar.width(0);           // 重置播放进度条为0
        time.removeClass('active');
        tProgress.text('00:00');    // 播放时间重置
        totalTime.text('00:00');    // 总时间重置

        // 获取当前索引的:歌曲名，歌手名，图片，歌曲链接等信息
        currMusic = musicNameData[currIndex];
        // currArtist = artistNameData[currIndex];
        currImg = musicImgsData[0];
        console.log("当前播放的 currIndex "+currIndex+" name:"+musicUrls[currIndex])
        audio.src = musicUrls[currIndex]; 
        //audio.src = '/Account/GetMusic?currIndex=' + currIndex
        nTime = 0;
        bTime = new Date();
        bTime = bTime.getTime();

        // 如果点击的是上一首/下一首 则设置开始播放，添加相关类名，重新开启定时器
        //if(flag != 0){
        //    audio.play();
        //    playerContent1.addClass('active');
        //    musicImgs.addClass('active');
        
        //    clearInterval(buffInterval);
        //    checkBuffering();
        //}
        //首次进入没有办法播放 
        audio.play();
        playerContent1.addClass('active');
        musicImgs.addClass('active'); 
        clearInterval(buffInterval);
        checkBuffering();
        // 将歌手名，歌曲名，图片链接，设置到元素上
        // artistName.text(currArtist);
        musicName.text(currMusic);
        musicImgs.find('.img').css({ 'background': 'url(' + currImg +') center / cover no-repeat'})
        
    }

    function selectTrack2(Index) {
        currIndex = Index;
		// playPauseBtn.attr('class','btn play-pause icon-jiediankaishi iconfont'); // 显示播放图标  
        musicImgs.removeClass('buffering');
        playPauseBtn.attr('class','btn play-pause icon-zanting iconfont') // 显示暂停图标
        seekBar.width(0);           // 重置播放进度条为0
        time.removeClass('active');
        tProgress.text('00:00');    // 播放时间重置
        totalTime.text('00:00');    // 总时间重置 
        // 获取当前索引的:歌曲名，歌手名，图片，歌曲链接等信息
        currMusic = musicNameData[currIndex];
        // currArtist = artistNameData[currIndex];
        currImg = musicImgsData[0];
        
        audio.src = musicUrls[currIndex]; 
        nTime = 0;
        bTime = new Date();
        bTime = bTime.getTime(); 
        // 如果点击的是上一首/下一首 则设置开始播放，添加相关类名，重新开启定时器 
		audio.play();
		playerContent1.addClass('active');
		musicImgs.addClass('active'); 
		clearInterval(buffInterval);
		checkBuffering();  
        // 将歌手名，歌曲名，图片链接，设置到元素上
        // artistName.text(currArtist);
        musicName.text(currMusic);
    musicImgs.find('.img').css({ 'background': 'url(' + currImg +') center / cover no-repeat'})
    }
    // 初始化函数
    audio = new Audio();  // 创建Audio对象
    function initPlayer() {
        playPauseBtn.off('click'); // 移除点击事件监听器
		selectTrack(0);       // 初始化第一首歌曲的相关信息
		audio.loop = false;   // 取消歌曲的循环播放功能
		
        playPauseBtn.on('click',playPause); // 点击播放/暂停 按钮，触发playPause函数
        
		// 进度条 移入/移出/点击 动作触发相应函数
		sArea.mousemove(function(event){ showHover(event); }); 
        sArea.mouseout(hideHover);
        sArea.on('click',playFromClickedPos);
        
        // 实时更新播放时间
        $(audio).on('timeupdate',updateCurrTime); 

         // 上下首切换
        playPrevBtn.off('click'); // 先解绑之前的上一首切换事件处理程序 
        playPrevBtn.on('click', function () { selectTrack(-1); });
        playNextBtn.off('click'); // 先解绑之前的下一首切换事件处理程序
        playNextBtn.on('click', function () { selectTrack(1); });
        // 音量控制
        volumeSlider = $('#volume-slider'); // 假设有一个 id 为 volume-slider 的音量滑块
        volumeSlider.on('input', function () {
            var volume = $(this).val();
            audio.volume = volume / 100; // 设置音量（范围为0到1）
        });
    } 
    //// 调用初始化函数
    //initPlayer();

    // 获取 SVG 元素
    var loopControl = document.getElementById('loopControl');
    // 添加点击事件监听器
    loopControl.addEventListener('click', function () {
        // 切换 SVG 图标
        
        toggleSVG();
    });
    // 切换 SVG 图标函数
    function toggleSVG() {
        // 获取当前 SVG 的路径内容
        const currentPath = loopControl.querySelector('path').getAttribute('d');
        // 判断当前 SVG 是否为第一个图标
        if (currentPath === "M9.333 9.333h13.333v4l5.333-5.333-5.333-5.333v4h-16v8h2.667v-5.333zM22.667 22.667h-13.333v-4l-5.333 5.333 5.333 5.333v-4h16v-8h-2.667v5.333z") {
            // 如果是第一个图标，则切换为第二个图标 单曲循环
            console.log("单曲循环播放")
            randomusic = false;
            loopmusic = true;
            loopControl.setAttribute("viewBox", "0 0 1024 1024");
            loopControl.innerHTML = `
            <path d="M64 682.666667a21.333333 21.333333 0 0 1-21.333333-21.333334V224a53.393333 53.393333 0 0 1 53.333333-53.333333h812.5l-48.92-48.913334a21.333333 21.333333 0 0 1 30.173333-30.173333l85.333334 85.333333a21.333333 21.333333 0 0 1 0 30.173334l-85.333334 85.333333a21.333333 21.333333 0 0 1-30.173333-30.173333l48.92-48.913334H96a10.666667 10.666667 0 0 0-10.666667 10.666667v437.333333a21.333333 21.333333 0 0 1-21.333333 21.333334z m100.42 249.753333a21.333333 21.333333 0 0 0 0-30.173333L115.5 853.333333H928a53.393333 53.393333 0 0 0 53.333333-53.333333V362.666667a21.333333 21.333333 0 0 0-42.666666 0v437.333333a10.666667 10.666667 0 0 1-10.666667 10.666667H115.5l48.92-48.913334a21.333333 21.333333 0 0 0-30.173333-30.173333l-85.333334 85.333333a21.333333 21.333333 0 0 0 0 30.173334l85.333334 85.333333a21.333333 21.333333 0 0 0 30.173333 0zM554.666667 618.666667V405.333333a21.333333 21.333333 0 0 0-21.333334-21.333333h-42.666666a21.333333 21.333333 0 0 0 0 42.666667h21.333333v192a21.333333 21.333333 0 0 0 42.666667 0z"></path>
        `;
        } else if (currentPath === "M64 682.666667a21.333333 21.333333 0 0 1-21.333333-21.333334V224a53.393333 53.393333 0 0 1 53.333333-53.333333h812.5l-48.92-48.913334a21.333333 21.333333 0 0 1 30.173333-30.173333l85.333334 85.333333a21.333333 21.333333 0 0 1 0 30.173334l-85.333334 85.333333a21.333333 21.333333 0 0 1-30.173333-30.173333l48.92-48.913334H96a10.666667 10.666667 0 0 0-10.666667 10.666667v437.333333a21.333333 21.333333 0 0 1-21.333333 21.333334z m100.42 249.753333a21.333333 21.333333 0 0 0 0-30.173333L115.5 853.333333H928a53.393333 53.393333 0 0 0 53.333333-53.333333V362.666667a21.333333 21.333333 0 0 0-42.666666 0v437.333333a10.666667 10.666667 0 0 1-10.666667 10.666667H115.5l48.92-48.913334a21.333333 21.333333 0 0 0-30.173333-30.173333l-85.333334 85.333333a21.333333 21.333333 0 0 0 0 30.173334l85.333334 85.333333a21.333333 21.333333 0 0 0 30.173333 0zM554.666667 618.666667V405.333333a21.333333 21.333333 0 0 0-21.333334-21.333333h-42.666666a21.333333 21.333333 0 0 0 0 42.666667h21.333333v192a21.333333 21.333333 0 0 0 42.666667 0z") {
            console.log("随机播放")
            randomusic = true;
            loopmusic = false;
            loopControl.setAttribute("viewBox", "0 0 1024 1024");
            loopControl.innerHTML = `
            <path d="M914.2 705L796.4 596.8c-8.7-8-22.7-1.8-22.7 10V688c-69.5-1.8-134-39.7-169.3-99.8l-45.1-77 47-80.2c34.9-59.6 98.6-97.4 167.4-99.8v60.1c0 11.8 14 17.9 22.7 10l117.8-108.1c5.8-5.4 5.8-14.6 0-19.9L796.4 165c-8.7-8-22.7-1.8-22.7 10v76H758c-4.7 0-9.3 0.8-13.5 2.3-36.5 4.7-72 16.6-104.1 35-42.6 24.4-78.3 59.8-103.1 102.2L513 432l-24.3-41.5c-24.8-42.4-60.5-77.7-103.1-102.2C343 263.9 294.5 251 245.3 251H105c-22.1 0-40 17.9-40 40s17.9 40 40 40h140.3c71.4 0 138.3 38.3 174.4 99.9l47 80.2-45.1 77c-36.2 61.7-103 99.9-174.4 99.9H105c-22.1 0-40 17.9-40 40s17.9 40 40 40l142 0.1h0.2c49.1 0 97.6-12.9 140.2-37.3 42.7-24.4 78.3-59.8 103.2-102.2l22.4-38.3 22.4 38.3c24.8 42.4 60.5 77.8 103.2 102.2 33.1 18.9 69.6 30.9 107.3 35.4 3.8 1.2 7.8 1.8 11.9 1.8l15.9 0.1v55c0 11.8 14 17.9 22.7 10L914.2 725c5.9-5.5 5.9-14.7 0-20z"></path>
        `;
        } else {
            // 修改viewBox属性的值
            console.log("不循环 不随机 播放")
            randomusic = false;
            loopmusic = false; 
            loopControl.setAttribute("viewBox", "0 0 29 32");
            // 如果不是第一个图标，则切换回第一个图标
            loopControl.innerHTML = `
            <path d="M9.333 9.333h13.333v4l5.333-5.333-5.333-5.333v4h-16v8h2.667v-5.333zM22.667 22.667h-13.333v-4l-5.333 5.333 5.333 5.333v-4h16v-8h-2.667v5.333z"></path>
        `;
        }
    } 
  
    //// 当鼠标进入 #player-content2 区域时显示 #albumList
    //$('#player-content2').mouseenter(function () {
    //    $('#albumList').
    //});

    //// 当鼠标离开 #player-content2 区域时隐藏 #albumList
    //$('#player-content2').mouseleave(function () {
    //    $('#albumList').hide();
    //});




});
var musicControl = document.getElementById('musicControl');
musicControl.addEventListener('click', function () {
    // 在这里编写点击事件的处理代码
    console.log('Music Control被点击了！');
});
