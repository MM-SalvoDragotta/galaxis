module.exports = function (grunt) {
  
  require('time-grunt')(grunt);
  
  grunt.initConfig({
    
    jitGrunt: {
      staticMappings: {
          notify_hooks: 'grunt-notify'
      }
    },
    
    sass: {
      options: {
          sourceMap: true,
          includePaths: require('node-bourbon').includePaths
      },
      dist: {
        files: {
          'css/screen.css': 'scss/screen.scss'
        }
      }
    },

    
    // Configuration for grunt-browser-sync
    // See http://www.browsersync.io/docs/grunt/
    browserSync: {
      files: {
        src: 'css/**/*.css'
      }
    },
    
    concurrent: {
      dev: {
        tasks: [
          'watch',
          'browserSync'
        ],
        options: {
          logConcurrentOutput: true
        }
      },
    },

    watch: {
      sass: {
        files: ['scss/**/*.scss'],
        tasks: ['sass'],
        options: {
          livereload: true
        }
      }
    },
    
    notify_hooks: {
      options: {
        success: true
      }
    },
    
    build: {
      tasks: ['sass']
    }
    
  });

  grunt.loadNpmTasks('grunt-notify');
  grunt.loadNpmTasks('grunt-contrib-watch');
  grunt.loadNpmTasks('grunt-sass');
  grunt.loadNpmTasks('grunt-browser-sync');
  grunt.loadNpmTasks('grunt-concurrent');
  grunt.loadNpmTasks('grunt-contrib-copy');
  
  grunt.registerTask('default', ['sass']);
  
  grunt.task.run('notify_hooks');
  
 };