function main(splash)
    splash.js_enabled = false
    assert(splash:go(splash.args.url))
    assert(splash:wait(3))
    return {splash:html()}
end