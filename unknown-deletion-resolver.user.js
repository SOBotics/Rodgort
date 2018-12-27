// ==UserScript==
// @name         Unknown Deletion Resolver
// @namespace    http://tampermonkey.net/
// @version      0.1
// @description  try to take over the world!
// @author       You
// @match        */unknown-deletion-resolution
// @grant        GM_xmlhttpRequest
// ==/UserScript==

(function() {
    'use strict';
    unsafeWindow.getRevision = function(postId) {
        return new Promise((resolve, reject) => {
            GM_xmlhttpRequest({
                method: "GET",
                url: "https://stackoverflow.com/posts/" + postId + "/revisions",
                onload: function(response) {
                    resolve(response)
                }
            });
        });
    }
})();