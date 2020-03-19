(function () {
    'use strict';

    const data = {
        drawer: null,
        goDark: true,
        videoPlayerShow: false,
        videoPlayer: null,
        movies: {
            all: [],
            recentlyAdded: []
        },
        movie: {},
        rigthDrawer: false
    };

    const watch = {
    }

    const methods = {
        showOverview(movie) {
            this.rigthDrawer = true;
            this.movie = movie;
        },
        getMovies() {
            getMovies();
        },
        playMovie(urlVideo) {
            this.videoPlayerShow = true;
            if (this.videoPlayer == null) {
                $('#video-player').ready(function () {
                    app.videoPlayer = amp('video-player', { /* Options */
                        techOrder: ["azureHtml5JS", "flashSS", "html5FairPlayHLS", "silverlightSS", "html5"],
                        "nativeControlsForTouch": false,
                        autoplay: true,
                        controls: true,
                        width: "640",
                        height: "400",
                        poster: ""
                    }, function () {
                        console.log('Good to go!');
                        // add an event listener
                        this.addEventListener('ended', function () {
                            console.log('Finished!');
                        })
                    })

                    app.videoPlayer.src([{
                        src: urlVideo
                    }]);
                });
            } else {
                this.videoPlayer.src([{
                    src: urlVideo
                }]);
            }
        },
        stopVideoPlayer() {
            this.videoPlayer.pause();
            this.videoPlayerShow = false;
        }
    }

    Vue.filter('formatDate', function (value) {
        if (!value) return ''
        return moment(String(value)).format('(YYYY)');
    });

    Vue.filter('formatTime', function (value) {
        if (!value) return ''
        var hours = (value / 60);
        var rhours = Math.floor(hours);
        var minutes = (hours - rhours) * 60;
        var rminutes = Math.round(minutes);
        return `${rhours} h ${rminutes} m `;
    });

    const app = new Vue({
        el: '#app',
        vuetify: new Vuetify({
            theme: {
                dark: true
            }
        }),
        data: data,
        watch: watch,
        methods: methods,
        computed: {
            setTheme() {
                return (this.$vuetify.theme.dark = this.goDark);
            }
        }
    });

    const apiBaseUrl = 'https://<your domain>.azurewebsites.net';

    getConnectionInfo().then(info => {
        info.accessToken = info.accessToken || info.accessKey;
        info.url = info.url || info.endpoint;
        data.ready = true;
        const options = {
            accessTokenFactory: () => info.accessToken
        };
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(info.url, options)
            .configureLogging(signalR.LogLevel.Information)
            .build();
        connection.on('newMovie', newMovie);
        connection.onclose(() => console.log('disconnected'));
        console.log('connecting...');
        connection.start()
            .then(() => console.log('connected!'))
            .catch(console.error);
    }).catch(alert);

    getMovies();

    function getAxiosConfig() {
        const config = {
            headers: {}
        };

        return config;
    }

    function getConnectionInfo() {
        return axios.post(`${apiBaseUrl}/api/negotiate`, null, getAxiosConfig())
            .then(resp => resp.data);
    }

    function newMovie(movie) {
        app.movies.recentlyAdded.unshift(movie);
        app.movies.all.push(movie);
    }

    function getMovies() {
        axios.get('/api/movies').then(onSuccessMovies);
    }

    function onSuccessMovies(response) {
        app.movies = response.data;
        console.log(app.movies);
    }
})();