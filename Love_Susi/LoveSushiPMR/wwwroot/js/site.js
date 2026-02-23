// LoveSushi PMR - Main JavaScript

$(document).ready(function() {
    // Initialize jQuery validation
    initializeFormValidation();
    
    // Initialize cart count on page load
    updateCartCount();
    
    // Sticky navbar effect
    $(window).scroll(function() {
        if ($(window).scrollTop() > 50) {
            $('.navbar').addClass('scrolled');
        } else {
            $('.navbar').removeClass('scrolled');
        }
    });
});

// Initialize form validation
function initializeFormValidation() {
    // jQuery Validation localization for Russian
    $.extend($.validator.messages, {
        required: "Это поле обязательно для заполнения",
        email: "Пожалуйста, введите корректный адрес электронной почты",
        minlength: jQuery.validator.format("Пожалуйста, введите не менее {0} символов"),
        maxlength: jQuery.validator.format("Пожалуйста, введите не более {0} символов"),
        number: "Пожалуйста, введите правильное число",
        equalTo: "Пожалуйста, введите то же значение еще раз",
        match: "Пожалуйста, введите значение еще раз",
        phone: "Пожалуйста, введите корректный номер телефона",
        url: "Пожалуйста, введите корректный URL"
    });

    // Validate all forms with novalidate attribute - only for UI feedback
    $('form[novalidate]').each(function() {
        console.log('[FORM] Initializing validation for form:', this);
        
        $(this).validate({
            errorClass: 'is-invalid',
            validClass: 'is-valid',
            errorElement: 'div',
            errorPlacement: function(error, element) {
                error.addClass('invalid-feedback d-block');
                error.insertAfter(element);
            },
            highlight: function(element) {
                $(element).addClass('is-invalid').removeClass('is-valid');
            },
            unhighlight: function(element) {
                $(element).removeClass('is-invalid').addClass('is-valid');
            },
            rules: {
                Email: {
                    required: true,
                    email: true
                },
                Password: {
                    required: true,
                    minlength: 6
                },
                ConfirmPassword: {
                    required: true,
                    equalTo: '[name="Password"]'
                },
                Phone: {
                    required: true
                },
                Name: {
                    required: true
                }
            },
            messages: {
                ConfirmPassword: {
                    equalTo: "Пароли не совпадают"
                }
            }
        });
    });

    // Handle checkbox RememberMe properly
    $('#rememberMe').on('change', function() {
        console.log('[FORM] RememberMe checkbox changed:', this.checked);
    });
}



// Add to cart function
function addToCart(dishId, quantity = 1) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { dishId: dishId, quantity: quantity },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                updateCartBadge(response.totalItems);
                showNotification('Товар добавлен в корзину!', 'success');
            } else {
                showNotification(response.message, 'error');
            }
        },
        error: function() {
            showNotification('Ошибка при добавлении в корзину', 'error');
        }
    });
}

// Update cart quantity
function updateQuantity(cartItemId, change) {
    var currentQty = parseInt($('[data-cart-item-id="' + cartItemId + '"] .quantity').text());
    var newQty = currentQty + change;
    
    if (newQty < 1) {
        removeFromCart(cartItemId);
        return;
    }
    
    $.ajax({
        url: '/Cart/UpdateQuantity',
        type: 'POST',
        data: { cartItemId: cartItemId, quantity: newQty },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                location.reload();
            }
        },
        error: function() {
            showNotification('Ошибка при обновлении количества', 'error');
        }
    });
}

// Remove from cart
function removeFromCart(cartItemId) {
    if (!confirm('Удалить товар из корзины?')) return;
    
    $.ajax({
        url: '/Cart/RemoveFromCart',
        type: 'POST',
        data: { cartItemId: cartItemId },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                location.reload();
            }
        },
        error: function() {
            showNotification('Ошибка при удалении товара', 'error');
        }
    });
}

// Clear cart
function clearCart() {
    if (!confirm('Очистить корзину?')) return;
    
    $.ajax({
        url: '/Cart/ClearCart',
        type: 'POST',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                location.reload();
            }
        },
        error: function() {
            showNotification('Ошибка при очистке корзины', 'error');
        }
    });
}

// Update cart count
function updateCartCount() {
    $.ajax({
        url: '/Cart/GetCartCount',
        type: 'GET',
        success: function(response) {
            updateCartBadge(response.count);
        }
    });
}

// Update cart badge
function updateCartBadge(count) {
    $('#cart-count').text(count);
    if (count > 0) {
        $('#cart-count').show();
    } else {
        $('#cart-count').hide();
    }
}

// Show notification
function showNotification(message, type = 'info') {
    var icon = type === 'success' ? '✓' : type === 'error' ? '✗' : 'ℹ';
    var bgColor = type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#17a2b8';
    
    var notification = $('<div class="notification">')
        .css({
            'position': 'fixed',
            'top': '100px',
            'right': '20px',
            'background': bgColor,
            'color': 'white',
            'padding': '15px 25px',
            'border-radius': '8px',
            'box-shadow': '0 4px 20px rgba(0,0,0,0.2)',
            'z-index': '9999',
            'display': 'flex',
            'align-items': 'center',
            'gap': '10px',
            'font-weight': '500'
        })
        .html('<span style="font-size:1.2rem">' + icon + '</span> ' + message);
    
    $('body').append(notification);
    
    notification.animate({ opacity: 1, right: '30px' }, 300);
    
    setTimeout(function() {
        notification.animate({ opacity: 0, right: '10px' }, 300, function() {
            notification.remove();
        });
    }, 3000);
}

// Smooth scroll for anchor links
$('a[href^="#"]').on('click', function(e) {
    e.preventDefault();
    var target = $(this.getAttribute('href'));
    if (target.length) {
        $('html, body').animate({
            scrollTop: target.offset().top - 80
        }, 800);
    }
});

// Input mask for phone
$(document).on('focus', 'input[type="tel"]', function() {
    $(this).attr('placeholder', '+373 (XXX) XX-XXX');
});

// Form validation enhancement
$('form').on('submit', function() {
    var $form = $(this);
    var $submitBtn = $form.find('button[type="submit"]');
    $submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Загрузка...');
});

// Image preview for file inputs
$(document).on('change', 'input[type="file"]', function() {
    var file = this.files[0];
    if (file) {
        var reader = new FileReader();
        reader.onload = function(e) {
            var preview = $('<img>')
                .attr('src', e.target.result)
                .css({
                    'max-width': '200px',
                    'max-height': '200px',
                    'border-radius': '8px',
                    'margin-top': '10px'
                });
            
            var $container = $(this).closest('.mb-3');
            $container.find('.image-preview').remove();
            $container.append($('<div class="image-preview">').append(preview));
        }.bind(this);
        reader.readAsDataURL(file);
    }
});
