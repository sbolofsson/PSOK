var kademlia = kademlia || {};
kademlia.ajaxmanager = {
    retrieve: function(url, opts) {
        $.ajax({
            dataType: 'json',
            url: url,
            cache: false,
            async: true,
            type: opts.type || 'GET',
            contentType: 'application/json; charset=utf-8',
            data: opts.data,
            success: opts.success || $.noop,
            error: opts.error || $.noop
        });
    }
};